using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Management.Automation.Language;
using PSTonberry.Model;
using Tonberry.Core.Model;

namespace PSTonberry;

internal static class DataExtensions
{
    public static PSTonberryConfiguration GetConfiguration(this PSDataFile dataFile, DirectoryInfo directory)
    {
        if (dataFile.HasPrivateData && dataFile.PrivateData.IsTonberryEnabled)
        {
            if (dataFile.PrivateData.TonberryData.TryGetSafeValue(out Hashtable tonberryData))
            {
                var converter = TypeDescriptor.GetConverter(typeof(TonberryConfiguration));
                return (PSTonberryConfiguration)converter.ConvertFrom(null, null, tonberryData);
            }
        }

        return new PSTonberryConfiguration(dataFile, directory);
    }

    public static void GetCommentedText(this PSTokenizedLine tokenizedLine)
    {
        tokenizedLine.CommentContent = new PSTokenizedLine(tokenizedLine.Line, tokenizedLine.Indent);
        if (!tokenizedLine.IsMultiLineComment && !tokenizedLine.IsSingleLineComment)
        {
            return;
        }

        int newIndex = 0;
        foreach (var dataToken in tokenizedLine)
        {
            ProcessToken(dataToken, tokenizedLine.CommentContent, ref newIndex);
        }

        tokenizedLine.CommentContent.TrackChanges = true;
    }

    public static List<PSTokenizedLine> ToTokenizedLines(this Token[] tokens)
    {
        var tokenizedCollection = new List<PSTokenizedLine>();
        if (tokens is not null && tokens.Length != 0)
        {
            foreach (var tokenizedLine in tokens.GetPSDataFileLine())
            {
                tokenizedLine.TrackChanges = true;
                if (tokenizedLine.IsComment)
                {
                    tokenizedLine.GetCommentedText();
                }

                tokenizedCollection.Add(tokenizedLine);
            }
        }

        return tokenizedCollection;
    }

    private static IEnumerable<PSTokenizedLine> GetPSDataFileLine(this Token[] tokens)
    {
        int indent = 0;
        int index = 0;
        int line = 1;
        var tokenCollection = new PSTokenizedLine(line, indent);
        for (int i = 0; i < tokens.Length; i++)
        {
            tokenCollection.Add(tokens[i].CreateDataToken(index));
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
                tokenCollection = new PSTokenizedLine(line, indent);
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

    private static void ProcessToken(IPSTokenEntry dataToken, PSTokenizedLine comment, ref int newIndex)
    {
        if (dataToken.IsComment)
        {
            ProcessComment(dataToken, comment, ref newIndex);
        }
        else
        {
            dataToken.Index = newIndex;
            comment.Add(dataToken);
            newIndex++;
        }
    }

    private static void ProcessComment(IPSTokenEntry dataToken, PSTokenizedLine comment, ref int newIndex)
    {
        var text = dataToken.Text.TrimStart('#').Trim();
        var ast = Parser.ParseFile(text, out Token[] tokens, out ParseError[] errors);
        if (errors.Length > 0)
        {
            throw new InvalidOperationException(string.Format(Resources.TextCouldNotBeParsed, text));
        }

        ParseCommentText(comment, tokens, ref newIndex);
    }

    private static void ParseCommentText(PSTokenizedLine comment, Token[] tokens, ref int newIndex)
    {
        foreach (var token in tokens)
        {
            if (token.Kind is TokenKind.EndOfInput)
            {
                continue;
            }

            var dataToken = token.CreateDataToken(newIndex);
            comment.Add(dataToken);
            newIndex++;
        }
    }
}