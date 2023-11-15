using System.IO;
using System.Management.Automation.Language;

namespace PSTonberry.Model;

internal class PSDataFile
{
    public PSDataFileAst<HashtableAst> DataFile { get; internal set; }

    public DirectoryInfo ModuleDirectory;

    public string ModuleName;

    public string ModulePath;

    public PSDataFileAst<HashtableAst> TonberryData { get; internal set; }

    public bool IsTonberryEnabled => TonberryData is not null;

    public bool IsValid => DataFile is not null;

    internal PSDataFile(string moduleName, DirectoryInfo directory) : base()
    {
        ModuleDirectory = directory;
        ModuleName = moduleName;
    }

    internal PSDataFile(PSDataFileAst<HashtableAst> dataFileAst,
                        string filePath,
                        string moduleName,
                        DirectoryInfo directory) : this(moduleName, directory)
    {
        DataFile = dataFileAst;
        ModulePath = filePath;
    }

    internal PSDataFile(PSDataFileAst<HashtableAst> dataFileAst,
                        PSDataFileAst<HashtableAst> tonberryConfigAst,
                        string filePath,
                        string moduleName,
                        DirectoryInfo directory) : this(dataFileAst, filePath, moduleName, directory)
    {
        TonberryData = tonberryConfigAst;
    }
}