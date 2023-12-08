namespace PSDataFileHelper;

public interface IPSDataFileValue<T> : IPSDataFileObject
{
    bool HasValue { get; }

    T GetValue();

    void SetValue(T value);

    bool TrySetValue(object value);
}