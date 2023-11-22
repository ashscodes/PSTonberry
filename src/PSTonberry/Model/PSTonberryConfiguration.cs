using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using Tonberry.Core.Model;
using YamlDotNet.Serialization;

namespace PSTonberry.Model;

public sealed class PSTonberryConfiguration : TonberryConfiguration
{
    internal PSDataFile DataFile { get; set; }

    [YamlIgnore]
    public PSTonberryConfigType Type { get; internal set; } = PSTonberryConfigType.Yaml;

    public PSTonberryConfiguration(DirectoryInfo directory) : base(directory) { }

    internal PSTonberryConfiguration(PSDataFile dataFile, DirectoryInfo root) : base(root)
    {
        DataFile = dataFile;
        if (DataFile.HasPrivateData && DataFile.PrivateData.IsTonberryEnabled)
        {
            Type = PSTonberryConfigType.DataFile;
        }
    }

    public override void Save()
    {
        if (Type == PSTonberryConfigType.DataFile)
        {
            SetDataFileValues();
            DataFile.Write();
            return;
        }

        base.Save();
    }

    private void SetDataFileValues()
    {
        if (Type != PSTonberryConfigType.DataFile)
        {
            return;
        }

        var converter = TypeDescriptor.GetConverter(typeof(TonberryConfiguration));
        if (converter is not null && converter.CanConvertTo(typeof(Hashtable)))
        {
            var hashtable = (Hashtable)converter.ConvertTo(null,
                                                           CultureInfo.InvariantCulture,
                                                           this,
                                                           typeof(Hashtable));

            if (DataFile.PrivateData.TryUpdateTonberryData((PSHashtable)hashtable))
            {
                return;
            }
        }

        throw new ArgumentException(Resources.CouldNotConvertConfiguration);
    }
}

public enum PSTonberryConfigType
{
    DataFile,

    Yaml,
}