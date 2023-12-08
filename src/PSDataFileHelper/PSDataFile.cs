using System.Collections.Generic;
using System.IO;

namespace PSDataFileHelper;

public sealed class PSDataFile : PSDataFileMap
{
    private DirectoryInfo _moduleDirectory;

    private FileInfo _modulePath;

    public List<IPSDataFileObject> AdditionalData { get; set; } = [];

    public PrivateData PrivateData { get; set; }

    public PSDataFile(string filePath) : base()
    {
        _modulePath = new FileInfo(filePath);
        _moduleDirectory = _modulePath?.Directory;
    }
}