using System.IO;
using Tonberry.Core.Model;

namespace PSTonberry.Model;

public sealed class PSTonberryConfiguration : TonberryConfiguration
{
    internal PSDataFile DataFile { get; set; }

    public PSTonberryConfiguration(DirectoryInfo directory) : base(directory)
    {

    }

    public static PSTonberryConfiguration ReadConfig(string path) => ReadConfig(new DirectoryInfo(path));

    public static PSTonberryConfiguration ReadConfig(DirectoryInfo directory)
    {
        // To be completed
        return null;
    }
}