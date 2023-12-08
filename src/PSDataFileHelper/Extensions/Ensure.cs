using System;
using System.Collections;
using System.Diagnostics;

namespace PSDataFileHelper;

[DebuggerStepThrough]
internal static class Ensure
{
    public static void ArrayNotEmpty(object[] value, string name)
    {
        if (value is null)
        {
            throw new ArgumentNullException(name, string.Format(Resources.ObjectIsNull, name));
        }

        if (value.Length < 1)
        {
            throw new ArgumentNullException(name, string.Format(Resources.ArrayIsEmpty, name));
        }
    }

    public static void CollectionNotEmpty(ICollection value, string name)
    {
        if (value is null)
        {
            throw new ArgumentNullException(name, string.Format(Resources.ObjectIsNull, name));
        }

        if (value.Count < 1)
        {
            throw new ArgumentNullException(name, string.Format(Resources.CollectionIsEmpty, name));
        }
    }

    public static void IndexIsInRange(int index, int count)
    {
        if (index < 0 || index >= count)
        {
            throw new ArgumentOutOfRangeException(string.Format(Resources.ArgumentOutsideOfIndex, index, count));
        }
    }

    public static void StringNotNullOrEmpty(string value, string propertyName)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(string.Format(Resources.StringIsNullOrEmpty, propertyName));
        }
    }

    public static void ValueNotNull(object value, string name)
    {
        if (value is null)
        {
            throw new ArgumentNullException(name, string.Format(Resources.ObjectIsNull, name));
        }
    }
}