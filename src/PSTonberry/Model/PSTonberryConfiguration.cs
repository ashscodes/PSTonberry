using System.IO;
using Tonberry.Core.Model;
using YamlDotNet.Serialization;

namespace PSTonberry.Model;

public sealed class PSTonberryConfiguration : TonberryConfiguration
{
    internal PSDataFile DataFile { get; set; }

    [YamlIgnore]
    public PSTonberryConfigType Type { get; private set; } = PSTonberryConfigType.Yaml;

    public PSTonberryConfiguration(DirectoryInfo directory) : base(directory) { }

    internal PSTonberryConfiguration(PSDataFile dataFile) : base(dataFile.ModuleDirectory)
    {
        DataFile = dataFile;
        if (DataFile.IsValid)
        {
            Type = PSTonberryConfigType.DataFile;
        }
    }

    public override void Save()
    {
        if (Type == PSTonberryConfigType.DataFile)
        {
            SetDataFileValues();
            PSDataFileConverter.WriteDataFile(DataFile);
            return;
        }

        base.Save();
    }

    private void SetDataFileValues() { }
}

public enum PSTonberryConfigType
{
    DataFile,
    Yaml,
}