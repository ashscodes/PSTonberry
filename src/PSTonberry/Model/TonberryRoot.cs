using System.IO;

namespace PSTonberry.Model;

public class TonberryRoot
{
    public DirectoryInfo Directory { get; set; }

    public string ModuleName { get; set; }

    public TonberryRoot(string moduleName, string projectRoot)
    {
        Directory = new DirectoryInfo(projectRoot);
        ModuleName = moduleName;
    }
}