using System.Management.Automation.Language;

namespace PSDataFileHelper;

internal class Resources
{
    public const string ArgumentOutsideOfIndex = "The value provided '{0}' was outside the total values '{1}'.";

    public const string ArrayIsEmpty = "The value provided for property '{0}' is an empty array.";

    public const string CollectionIsEmpty = "The value provided for property '{0}' is an empty collection.";

    public const string CouldNotSetValue = "The value provided could not be cast to the specific type '{0}'.";

    public const string DataFileInvalid = "The data file '{0}' could not be parsed.";

    public const string ObjectIsNull = "The value provided for property '{0}' is null.";

    public const string StringIsNullOrEmpty = "The value provided for property '{0}' is null or empty.";

    #region Tokens

    public static TokenKind[] StringTokens =
    [
        TokenKind.StringExpandable,
        TokenKind.StringLiteral,
    ];

    public static TokenKind[] ValueTokens =
    [
        TokenKind.Comment,
        TokenKind.HereStringExpandable,
        TokenKind.HereStringLiteral,
        TokenKind.Number,
        TokenKind.SplattedVariable,
        TokenKind.StringExpandable,
        TokenKind.StringLiteral,
        TokenKind.Variable
    ];

    #endregion Tokens
}