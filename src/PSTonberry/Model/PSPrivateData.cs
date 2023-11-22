using System.IO;

namespace PSTonberry.Model;

internal class PSPrivateData : PSHashtableData
{
    public bool IsTonberryEnabled => TonberryData is not null && !TonberryData.HasError;

    public PSTonberryData TonberryData { get; set; }

    public PSPrivateData(string privateData)
    {
        if (TryParseSection(privateData, Resources.TonberryData, out string tonberryDataStr))
        {
            TonberryData = new PSTonberryData(tonberryDataStr);
            privateData = privateData.Replace(tonberryDataStr, null);
        }

        Init(privateData);
    }

    public override void Write(StreamWriter writer)
    {
        throw new System.NotImplementedException();
    }

    internal bool TryUpdateTonberryData(PSHashtable tonberryConfig)
    {
        try
        {
            TonberryData = new PSTonberryData(tonberryConfig.ToString());
            return true;
        }
        catch
        {
            return false;
        }
    }
}