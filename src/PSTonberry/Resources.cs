using System.Management.Automation.Language;

namespace PSTonberry;

internal static class Resources
{
    public const string ArgumentOutsideOfIndex = "The value provided '{0}' was outside the total values '{1}'.";

    public const string CouldNotConvertConfiguration = "Could not convert the open Tonberry Configuration back to .psd1 format.";

    public const string DataFileCouldNotBeParsed = "The data file '{0}' could not be parsed.";

    public const string DataSectionCouldNotBeParsed = "The '{0}' section of the data file could not be parsed.";

    public const string DataFileNotHashtable = "The data file '{0}' does not appear to be a hashtable and cannot be parsed.";

    public const string DataFileNotFound = "Could not find data file '{0}' when recursively searching from the current directory. Are you in the root directory of your project?";

    public const string InvalidType = "Cannot convert object of type '{0}' to '{1}'.";

    public const string ObjectIsNull = "The object named '{0}' of type '{1}' was null.";

    public const string PathNotFound = "No object exists at the specified path {0}.";

    public const string PrivateData = "PrivateData";

    public const string TextCouldNotBeParsed = "The text '{0}' could not be parsed by the language parser.";

    public const string TonberryConfigurationFile = "Tonberry.Config.psd1";

    public const string TonberryData = "TonberryData";

    #region Enums

    public static TokenKind[] ValueTokens =
    [
        TokenKind.Comment,
        TokenKind.HereStringExpandable,
        TokenKind.HereStringLiteral,
        TokenKind.Number,
        TokenKind.StringExpandable,
        TokenKind.StringLiteral,
    ];

    #endregion Enums

    #region PSVariables

    public const string TonberryConfig = "global:TonberryConfig";

    public const string TonberryRoot = "global:TonberryRoot";

    #endregion PSVariables
}