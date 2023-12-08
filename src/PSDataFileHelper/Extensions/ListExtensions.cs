using System.Collections;

namespace PSDataFileHelper;

public static class ListExtensions
{
    public static bool Move(this IList list, int currentIndex, int targetIndex)
    {
        Ensure.IndexIsInRange(currentIndex, list.Count);
        Ensure.IndexIsInRange(targetIndex, list.Count);

        var item = list[currentIndex];
        list.RemoveAt(currentIndex);
        if (targetIndex > currentIndex)
        {
            targetIndex--;
        }

        try
        {
            list.Insert(targetIndex, item);
            return true;
        }
        catch
        {
            list.Insert(currentIndex, item);
            return false;
        }
    }
}