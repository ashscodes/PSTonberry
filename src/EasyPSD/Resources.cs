namespace EasyPSD;

internal class Resources
{
    public const string ArgumentOutsideOfIndex = "The value provided '{0}' was outside the total values '{1}'.";

    public const string ArrayIsEmpty = "The value provided for property '{0}' is an empty array.";

    public const string CollectionIsEmpty = "The value provided for property '{0}' is an empty collection.";

    public const string CouldNotSetValue = "The value provided could not be cast to the specific type '{0}'.";

    public const string DataFileInvalid = "The data file '{0}' could not be parsed.";

    public const string DataFileReadError = "An item of type '{0}' could not be read at token index '{1}'.";

    public const string ObjectIsNull = "The value provided for property '{0}' is null.";

    public const string RangesNotEqual = "Cannot compare a set of items, because they do not have an equal number of comparisons. First '{0}'; Second '{1}'";

    public const string StringIsNullOrEmpty = "The value provided for property '{0}' is null or empty.";

    #region Operators

    public static readonly string[] ComparisonOperators =
    [
        "-eq",
        "-ieq",
        "-ceq",
        "-ne",
        "-ine",
        "-cne",
        "-gt",
        "-igt",
        "-cgt",
        "-ge",
        "-ige",
        "-cge",
        "-lt",
        "-ilt",
        "-clt",
        "-le",
        "-ile",
        "-cle",
        "-like",
        "-ilike",
        "-clike",
        "-notlike",
        "-inotlike",
        "-cnotlike",
        "-match",
        "-imatch",
        "-cmatch",
        "-notmatch",
        "-inotmatch",
        "-cnotmatch",
        "-replace",
        "-ireplace",
        "-creplace",
        "-contains",
        "-icontains",
        "-ccontains",
        "-notcontains",
        "-inotcontains",
        "-cnotcontains",
        "-in",
        "-notin",
        "-is",
        "-isnot"
    ];

    public static readonly string[] LogicalOperators =
    [
        "-and",
        "-or",
        "-xor"
    ];

    #endregion Operators
}