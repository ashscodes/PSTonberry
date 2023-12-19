using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;

namespace EasyPSD;

public static class TokenLookAhead
{

    #region TokenKindSets

    public static TokenKind[] BasicStringTokens =
    [
        TokenKind.StringExpandable,
        TokenKind.StringLiteral
    ];

    public static TokenKind[] CommentTokens = [TokenKind.Comment];

    public static TokenKind[] DoubleQuotedStringTokens =
    [
        TokenKind.HereStringExpandable,
        TokenKind.StringExpandable,
    ];

    public static TokenKind[] LineTerminator =
    [
        TokenKind.NewLine,
        TokenKind.Semi
    ];

    public static TokenKind[] NumberTokens = [TokenKind.Number];

    public static TokenKind[] SingleQuotedStringTokens =
    [
        TokenKind.HereStringLiteral,
        TokenKind.StringLiteral,
    ];

    public static TokenKind[] VariableTokens =
    [
        TokenKind.SplattedVariable,
        TokenKind.Variable
    ];

    #endregion TokenKindSets

    #region Unions

    public static IEnumerable<TokenKind> GetIdentifiers() => BasicStringTokens.Union([TokenKind.Identifier]);

    public static IEnumerable<TokenKind> GetSimpleValueTokens() => DoubleQuotedStringTokens.Union(NumberTokens)
                                                                                           .Union(SingleQuotedStringTokens)
                                                                                           .Union(VariableTokens);

    #endregion Unions

    #region LookAheads

    public static TokenKind[][] ArrayLiteral =
    [
        GetSimpleValueTokens().ToArray(),
        [TokenKind.Comma],
        GetSimpleValueTokens().ToArray()
    ];

    public static TokenKind[][] InlineComment =
    [
        CommentTokens,
        LineTerminator
    ];

    public static TokenKind[][] InlineCommentWithComma =
    [
        [TokenKind.Comma],
        CommentTokens,
        LineTerminator
    ];

    public static TokenKind[][] MapEntry =
    [
        GetIdentifiers().ToArray(),
        [TokenKind.Equals]
    ];

    public static TokenKind[][] SimpleValue = [GetSimpleValueTokens().ToArray()];

    #endregion LookAheads
}