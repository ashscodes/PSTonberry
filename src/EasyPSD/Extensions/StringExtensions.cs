using System;

namespace EasyPSD;

public static class StringExtensions
{
    public static int FindEndOfCollectionInString(this string value, char open, char close)
    {
        bool isInExpandableString = false;
        bool isInLiteralString = false;
        bool isInString = isInExpandableString || isInLiteralString;
        char expandable = '"';
        char literal = '\'';
        int opened = 0;
        int closed = 0;
        for (int i = 0; i < value.Length; i++)
        {
            if (value[i] == expandable && value[i - 1] != '\\')
            {
                isInExpandableString = isInExpandableString ? false : true;
            }

            if (value[i] == literal && value[i - 1] != '\\')
            {
                isInLiteralString = isInLiteralString ? false : true;
            }

            if (value[i] == open && !isInString)
            {
                opened++;
            }

            if (value[i] == close && !isInString)
            {
                closed++;
            }

            if (opened > 0 && opened == closed)
            {
                return opened;
            }
        }

        return opened == closed ? opened : -1;
    }

    public static int IndexOfN(this string value, char character, int occurrence)
    {
        if (occurrence < 0)
        {
            throw new ArgumentException("Must be a positive integer.", nameof(occurrence));
        }

        int offset = value.IndexOf(character);
        for (int i = 1; i < occurrence; i++)
        {
            if (offset == -1)
            {
                return offset;
            }

            offset = value.IndexOf(character, offset + 1);
        }

        return offset;
    }

    public static bool HasNamedMap(this string collection, string identifier, out string namedMap)
    {
        var startIndex = collection.IndexOf(identifier);
        if (startIndex != -1)
        {
            var endOfCollection = collection[startIndex..].FindEndOfCollectionInString('{', '}');
            if (endOfCollection >= 0)
            {
                var endIndex = collection.IndexOfN('}', endOfCollection) + 1;
                if (endIndex != -1)
                {
                    namedMap = collection[startIndex..endIndex];
                    return true;
                }
            }
        }

        namedMap = string.Empty;
        return false;
    }
}