namespace PSDataFileHelper;

public sealed class PrivateData : DataSection
{
    public PSData PSData
    {
        get => (PSData)GetSection(nameof(PSData));
    }

    public PrivateData() : base(nameof(PrivateData)) { }

    public override string ToString()
    {
        throw new System.NotImplementedException();
    }
}