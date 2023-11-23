namespace PSTonberry.Model;

internal class PSPrivateData : PSHashtableData
{
    public bool IsTonberryEnabled => TonberryData is not null && !TonberryData.HasError;

    public PSTonberryData TonberryData { get; set; }

    public PSPrivateData(string privateData)
    {
        if (TryParseSection(privateData, Resources.TonberryData, out string tonberryDataStr, out string newPrivateData))
        {
            TonberryData = new PSTonberryData(tonberryDataStr);
        }

        Init(newPrivateData);
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