using System.Management.Automation.Language;

namespace PSTonberry;

internal static class Resources
{
    public const string InvalidType = "Cannot convert object of type '{0}' to '{1}'.";

    public const string ModuleCouldNotBeParsed = "The data for module '{0}' could not be parsed.";

    public const string ModuleDataNotHashtable = "The data for module '{0}' does not appear to be a hashtable and cannot be parsed.";

    public const string ModuleNotFound = "Could not find module '{0}' when recursively searching from the current directory. Are you in the root directory of your project?";

    public const string PathNotFound = "No object exists at the specified path {0}.";

    public const string TextCouldNotBeParsed = "The text '{0}' could not be parsed by the language parser.";

    public const string TonberryData = "TonberryData";

    public const string TonberryDataCouldNotBeParsed = "The tonberry data for module '{0}' could not be parsed.";

    #region Enums

    public static TokenKind[] ValueTokens =
    [
        TokenKind.Number,
        TokenKind.HereStringExpandable,
        TokenKind.HereStringLiteral,
        TokenKind.StringExpandable,
        TokenKind.StringLiteral,
    ];

    #endregion Enums
}