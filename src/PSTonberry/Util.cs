using System.IO;

namespace PSTonberry;

internal static class Util
{
    public static void CreateDirectory(FileSystemInfo fileSystemInfo)
    {
        if (fileSystemInfo is FileInfo fileInfo)
        {
            Directory.CreateDirectory(fileInfo.Directory.FullName);
        }

        Directory.CreateDirectory(fileSystemInfo.FullName);
    }
}