using System;

namespace PSTonberry;

internal static class Ensure
{
    public static void IndexIsInRange(int index, int count)
    {
        if (index < 0 || index >= count)
        {
            throw new ArgumentOutOfRangeException(string.Format(Resources.ArgumentOutsideOfIndex, index, count));
        }
    }

    public static void ValueNotNull(object value, string name, string message)
    {
        if (value is null)
        {
            throw new ArgumentNullException(name, message);
        }
    }
}