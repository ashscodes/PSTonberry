using System.Linq;
using System.Management.Automation.Language;

namespace EasyPSD;

public static class TokenExtensions
{
    public static bool Is(this Token token, TokenKind kind)
    {
        if (token is null)
        {
            return false;
        }

        return token.Kind == kind;
    }

    public static bool Is(this Token token, TokenFlags flag)
    {
        if (token is null)
        {
            return false;
        }

        return token.TokenFlags.HasFlag(flag);
    }

    public static bool IsArrayClose(this Token token) => token.Is(TokenKind.RParen);

    public static bool IsArrayStart(this Token token) => token.IsOneOf(TokenKind.AtParen, TokenKind.LParen);

    public static bool IsComment(this Token token) => token.Is(TokenKind.Comment);

    public static bool IsKeyword(this Token token) => token.Is(TokenFlags.Keyword);

    public static bool IsComparisonOperator(this Token token) => token.Is(TokenFlags.BinaryPrecedenceComparison);

    public static bool IsLogicalOperator(this Token token) => token.Is(TokenFlags.BinaryPrecedenceLogical);

    public static bool IsMapClose(this Token token) => token.Is(TokenKind.RCurly);

    public static bool IsMapStart(this Token token) => token.Is(TokenKind.AtCurly);

    public static bool IsNewLine(this Token token) => token.IsOneOf(TokenKind.NewLine, TokenKind.Semi);

    public static bool IsNot(this Token token, TokenFlags flag)
    {
        if (token is null)
        {
            return true;
        }

        return !token.TokenFlags.HasFlag(flag);
    }

    public static bool IsNot(this Token token, TokenKind kind)
    {
        if (token is null)
        {
            return false;
        }

        return token.Kind != kind;
    }

    public static bool IsOneOf(this Token token, params TokenKind[] tokenKinds)
    {
        if (token is null)
        {
            return false;
        }

        if (tokenKinds is null || tokenKinds.Length == 0)
        {
            return false;
        }

        return tokenKinds.Contains(token.Kind);
    }

    public static bool IsScriptblockStart(this Token token) => token.Is(TokenKind.LCurly);
}