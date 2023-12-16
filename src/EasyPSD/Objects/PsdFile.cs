using System.IO;

namespace EasyPSD;

public sealed class PsdFile : PsdBaseMap
{
    private DirectoryInfo _moduleDirectory;

    private FileInfo _modulePath;

    public PsdCoreData Data { get; set; }

    public PsdFile(string filePath, string originalText) : base()
    {
        _modulePath = new FileInfo(filePath);
        _moduleDirectory = _modulePath?.Directory;
        Data = new PsdCoreData(originalText);
    }
}