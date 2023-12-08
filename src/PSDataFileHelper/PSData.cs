namespace PSDataFileHelper;

public sealed class PSData : PSDataFileSection
{
    public PSData() : base(nameof(PSData)) { }

    public PSData(string originalText) : base(nameof(PSData), originalText) { }
}