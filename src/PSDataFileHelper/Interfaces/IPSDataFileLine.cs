namespace PSDataFileHelper;

public interface IPSDataFileLine : IPSDataFileObject
{
    bool HasPrecedingEmptyLine { get; }
}