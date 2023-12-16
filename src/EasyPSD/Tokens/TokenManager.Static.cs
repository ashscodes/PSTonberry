using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;

namespace EasyPSD;

public partial class TokenManager
{
    #region LookAheads

    public static TokenKind[][] ArrayLiteral =
    [
        GetSimpleValueTokens().ToArray(),
        [TokenKind.Comma]
    ];

    public static TokenKind[][] InlineComment =
    [
        CommentTokens,
        LineTerminator
    ];

    public static TokenKind[][] NamedArray =
    [
        GetIdentifiers().ToArray(),
        [TokenKind.Equals],
        [TokenKind.AtParen]
    ];

    public static TokenKind[][] NamedMap =
    [
        GetIdentifiers().ToArray(),
        [TokenKind.Equals],
        [TokenKind.AtCurly]
    ];

    public static TokenKind[][] SimpleMapEntry =
    [
        GetIdentifiers().ToArray(),
        [TokenKind.Equals],
        GetSimpleValueTokens().ToArray()
    ];

    #endregion LookAheads

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

    public static IEnumerable<TokenKind> GetIdentifiers()
        => BasicStringTokens.Union([TokenKind.Identifier]);

    public static IEnumerable<TokenKind> GetSimpleValueTokens()
        => NumberTokens.Union(DoubleQuotedStringTokens)
                       .Union(SingleQuotedStringTokens)
                       .Union(VariableTokens);

    #endregion Unions
}