namespace PSDataFileHelper;

public sealed class PSDataFileCoreObject : IPSDataFileObject
{
    public bool IsCollection => false;

    public bool IsReadOnly => true;
}