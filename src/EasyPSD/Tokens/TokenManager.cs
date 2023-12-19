using System;
using System.Management.Automation.Language;

namespace EasyPSD;

public class TokenManager
{
    private Token[] _tokens;

    public int ConsecutiveNewLines;

    public int Count => _tokens is not null ? _tokens.Length : 0;

    public Token Current => (Count <= 0 || (Index < 0 || Index > Count)) ? null : this[Index];

    public bool HasPrecedingNewLine => Previous is not null ? Previous.Kind == TokenKind.NewLine : false;

    public int Index { get; private set; }

    public bool InScriptblock { get; private set; }

    public Token Next => Index + 1 > Count ? null : this[Index + 1];

    public Token Previous => Index - 1 < 0 ? null : this[Index - 1];

    public Token this[int index] => _tokens[index];

    public TokenManager() => ConsecutiveNewLines = Index = 0;

    public void ConsumeToken()
    {
        if (_tokens[Index].Kind is TokenKind.NewLine)
        {
            ConsecutiveNewLines++;
        }
        else
        {
            ConsecutiveNewLines = 0;
        }

        Index++;
    }

    public void ConsumeToken(int count)
    {
        for (int i = 0; i < count; i++)
        {
            ConsumeToken();
        }
    }

    public Token[] GetRange(int count) => GetRange(Index, count);

    public Token[] GetRange(int index, int count)
    {
        if (_tokens is null || _tokens.Length == 0)
        {
            return [];
        }

        Ensure.IndexIsInRange(index, _tokens.Length);
        return _tokens[index..(index + count)];
    }

    public bool IsArrayLiteral() => PeekTokens(TokenLookAhead.ArrayLiteral);

    public bool IsDoubleQuotedString() => PeekTokens([TokenLookAhead.DoubleQuotedStringTokens]);

    public bool IsInlineComment() =>
        PeekTokens(TokenLookAhead.InlineComment) || PeekTokens(TokenLookAhead.InlineCommentWithComma);

    public bool IsLineTerminator() => PeekTokens([TokenLookAhead.LineTerminator]);

    public bool IsMapEntry() => PeekTokens(TokenLookAhead.MapEntry);

    public bool IsSimpleValue() => PeekTokens(TokenLookAhead.SimpleValue);

    public bool PeekTokens(TokenKind[][] tokenKindSet)
    {
        if (tokenKindSet is null
            || tokenKindSet.Length == 0
            || _tokens.Length < (Index + tokenKindSet.Length + 1))
        {
            return false;
        }

        for (int i = 0; i < tokenKindSet.Length; i++)
        {
            var token = _tokens[Index + i];
            if (!token.IsOneOf(tokenKindSet[i]))
            {
                return false;
            }
        }

        return true;
    }

    public bool TryGetDataFile(string filePath, out string fileContent, out Exception error)
    {
        error = null;
        fileContent = string.Empty;
        try
        {
            var scriptblock = Parser.ParseFile(filePath, out Token[] tokens, out ParseError[] errors);
            if (errors.Length > 0)
            {
                error = new PsdReaderException(string.Format(Resources.DataFileInvalid, filePath));
                return false;
            }

            fileContent = scriptblock.ToString();
            _tokens = tokens;
            return true;
        }
        catch (Exception ex)
        {
            error = ex;
            return false;
        }
    }
}