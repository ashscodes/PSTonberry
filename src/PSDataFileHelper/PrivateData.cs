namespace PSDataFileHelper;

public sealed class PrivateData : PSDataFileSection
{
    public PSData PSData { get; set; }

    public PrivateData() : base(nameof(PrivateData)) { }

    public PrivateData(string originalText) : base(nameof(PrivateData), originalText) { }
}