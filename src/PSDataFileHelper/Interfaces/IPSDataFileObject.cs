namespace PSDataFileHelper;

public interface IPSDataFileObject
{
    bool IsCollection { get; }

    bool IsReadOnly { get; }
}