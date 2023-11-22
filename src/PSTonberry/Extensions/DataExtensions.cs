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

    public static bool TryGetCommentedText(this PSTokenizedLine dataFileLine, out PSTokenizedLine comment)
    {
        comment = new PSTokenizedLine(dataFileLine.Line, dataFileLine.Indent);
        if (!dataFileLine.IsMultiLineComment && !dataFileLine.IsSingleLineComment)
        {
            return false;
        }

        int newIndex = 0;
        foreach (var dataToken in dataFileLine)
        {
            ProcessToken(dataToken, comment, ref newIndex);
        }

        comment.TrackChanges = true;
        return true;
    }

    public static List<PSTokenizedLine> ToTokenizedLines(this Token[] tokens)
    {
        var tokenCollections = new List<PSTokenizedLine>();
        if (tokens is not null && tokens.Length != 0)
        {
            foreach (var tokenCollection in tokens.GetPSDataFileLine())
            {
                tokenCollection.TrackChanges = true;
                tokenCollections.Add(tokenCollection);
            }
        }

        return tokenCollections;
    }

    public static void Write(this PSDataFile dataFile)
    {
        var tempPath = Path.GetTempFileName();
        using (var writer = new StreamWriter(tempPath))
        {
            writer.AutoFlush = true;
        }
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

    private static void ProcessToken(PSTokenEntry dataToken, PSTokenizedLine comment, ref int newIndex)
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

    private static void ProcessComment(PSTokenEntry dataToken, PSTokenizedLine comment, ref int newIndex)
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

    private static void Write(this PSTokenEntry dataFileToken, StreamWriter writer)
    {
        if (dataFileToken.IsNewLine)
        {
            writer.WriteLine();
        }
        else
        {
            writer.Write(dataFileToken.ToString());
        }
    }
}