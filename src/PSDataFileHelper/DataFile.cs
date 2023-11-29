using System.IO;

namespace PSDataFileHelper;

public sealed class DataFile : AstMap
{
    private DirectoryInfo _moduleDirectory;

    private FileInfo _modulePath;

    public DataFileContent Data { get; set; }

    public DataFile(string filePath) : base()
    {
        _modulePath = new FileInfo(filePath);
        _moduleDirectory = _modulePath?.Directory;
    }

    public override string ToString()
    {
        throw new System.NotImplementedException();
    }
}

public class DataFileContent : AstMap
{
    public PrivateData PrivateData
    {
        get => (PrivateData)GetDataSection(nameof(PrivateData));
    }

    public override string ToString()
    {
        throw new System.NotImplementedException();
    }
}