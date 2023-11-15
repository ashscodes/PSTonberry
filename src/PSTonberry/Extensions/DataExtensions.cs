using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using PSTonberry.Model;

namespace PSTonberry;

internal static class DataExtensions
{
    public static bool TryGetCommentedText(this PSDataFileTokenCollection tokenCollection,
                                           out PSDataFileTokenCollection comment)
    {
        comment = new PSDataFileTokenCollection(tokenCollection.Line, tokenCollection.Indent);
        if (tokenCollection.IsMultiLineComment || tokenCollection.IsSingleLineComment)
        {
            foreach (var token in tokenCollection)
            {
                int newIndex = 0;
                if (token.IsNewLine)
                {
                    comment.Add(token.Token, newIndex);
                    newIndex++;
                    continue;
                }

                if (token.IsComment && token.Text.Contains('='))
                {
                    var text = token.Text.TrimStart('#').Trim();
                    var ast = Parser.ParseFile(text, out Token[] tokens, out ParseError[] errors);
                    if (errors.Length > 0)
                    {
                        throw new InvalidOperationException(string.Format(Resources.TextCouldNotBeParsed, text));
                    }

                    foreach (var psDataTokenCollection in tokens.GetPSDataTokenCollection())
                    {
                        foreach (var psDataToken in psDataTokenCollection)
                        {
                            if (psDataToken.Token.Kind is TokenKind.EndOfInput)
                            {
                                continue;
                            }

                            psDataToken.Index = newIndex;
                            comment.Add(psDataToken);
                            newIndex++;
                        }
                    }
                }
            }
        }

        comment.TrackChanges = true;
        return true;
    }

    public static List<PSDataFileTokenCollection> ToPSDataTokenCollection(this Token[] tokens)
    {
        var tokenCollections = new List<PSDataFileTokenCollection>();
        if (tokens is not null && tokens.Length != 0)
        {
            foreach (var tokenCollection in tokens.GetPSDataTokenCollection())
            {
                tokenCollection.TrackChanges = true;
                tokenCollections.Add(tokenCollection);
            }
        }

        return tokenCollections;
    }

    private static IEnumerable<PSDataFileTokenCollection> GetPSDataTokenCollection(this Token[] tokens)
    {
        int indent = 0;
        int index = 0;
        int line = 1;
        var tokenCollection = new PSDataFileTokenCollection(line, indent);
        for (int i = 0; i < tokens.Length; i++)
        {
            tokenCollection.Add(tokens[i], index);
            index++;
            if (tokens[i].Kind is TokenKind.EndOfInput)
            {
                yield return tokenCollection;
            }

            if (tokens[i].Kind is TokenKind.NewLine && tokens[i].Text.EndsWith(','))
            {
                continue;
            }

            if (tokens[i].Kind is TokenKind.NewLine)
            {
                yield return tokenCollection;
                line++;
                index = 0;
                tokenCollection = new PSDataFileTokenCollection(line, indent);
            }

            if (tokens[i].Kind is TokenKind.AtCurly)
            {
                indent += 2;
            }

            if (tokens[i].Kind is TokenKind.RCurly)
            {
                indent -= 2;
            }
        }
    }
}