using System;
using System.Linq;
using System.Management.Automation.Language;

namespace EasyPSD;

public partial class TokenManager
{
    private int _consecutiveNewLines;

    private Token[] _tokens;

    public int Count => _tokens is not null ? _tokens.Length : 0;

    public Token Current => (Count <= 0 || (Index < 0 || Index > Count)) ? null : this[Index];

    public int Index { get; private set; }

    public Token Last => Index - 1 < 0 ? null : this[Index - 1];

    public Token Next => Index + 1 > Count ? null : this[Index + 1];

    public Token this[int index] => _tokens[index];

    public TokenManager() => _consecutiveNewLines = Index = 0;

    public void ConsumeToken()
    {
        if (_tokens[Index].Kind is TokenKind.NewLine)
        {
            _consecutiveNewLines++;
        }
        else
        {
            _consecutiveNewLines = 0;
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

    public bool IsArrayLiteral() => PeekTokens(ArrayLiteral);

    public bool IsDataFileSection() => PeekTokens(NamedMap);

    public bool IsDoubleQuotedString() => PeekTokens([DoubleQuotedStringTokens]);

    public bool IsInlineComment() => PeekTokens(InlineComment);

    public bool IsLineTerminator() => PeekTokens([LineTerminator]);

    public bool IsNamedArray() => PeekTokens(NamedArray);

    public bool IsNamedMap() => PeekTokens(NamedMap);

    public bool IsSimpleMapEntry() => PeekTokens(SimpleMapEntry);

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
            var tokenKind = _tokens[Index + i].Kind;
            if (tokenKindSet[i].Length == 0 || !tokenKindSet[i].Contains(tokenKind))
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