namespace PSTonberry;

internal static class Resources
{
    public const string InvalidType = "Cannot convert object of type '{0}' to '{1}'.";

    public const string ModuleCouldNotBeParsed = "The data for module '{0}' could not be parsed.";

    public const string ModuleDataNotHashtable = "The data for module '{0}' does not appear to be a hashtable and cannot be parsed.";

    public const string ModuleNotFound = "Could not find module '{0}' when recursively searching from the current directory. Are you in the root directory of your project?";

    public const string PathNotFound = "No object exists at the specified path {0}.";
}