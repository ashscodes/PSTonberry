using System;

namespace PSDataFileHelper;

internal static class StringExtensions
{
    public static int CountNestedTokens(this string value, char open, char close)
    {
        int opened = 0;
        int closed = 0;
        for (int i = 0; i < value.Length; i++)
        {
            if (value[i] == open)
            {
                opened++;
            }

            if (value[i] == close)
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
        for (int i = 0; i < occurrence; i++)
        {
            if (offset == -1)
            {
                return offset;
            }

            offset = value.IndexOf(character, offset + 1);
        }

        return offset;
    }

    public static bool TryReplaceDataSection(this string collection,
                                           string sectionName,
                                           out string dataSection,
                                           out string modifiedCollection)
    {
        var startIndex = collection.IndexOf(sectionName);
        if (startIndex != -1)
        {
            var endOfCollection = collection.CountNestedTokens('{', '}');
            if (endOfCollection > 0)
            {
                var endIndex = collection.IndexOfN('}', endOfCollection) + 1;
                if (endIndex != -1)
                {
                    dataSection = collection[startIndex..endIndex];
                    modifiedCollection = collection.Replace(dataSection, null);
                    return true;
                }
            }
        }

        modifiedCollection = collection;
        dataSection = string.Empty;
        return false;
    }
}