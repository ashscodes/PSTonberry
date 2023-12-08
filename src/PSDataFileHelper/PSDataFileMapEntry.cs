using System;

namespace PSDataFileHelper;

public class PSDataFileMapEntry : IPSDataFileLine
{
    private IPSDataFileObject _value;

    public string Key { get; set; }

    public bool IsCollection => typeof(IPSDataFileCollection<>).IsAssignableFrom(_value.GetType());

    public bool IsReadOnly { get; set; }

    public bool HasPrecedingEmptyLine { get; set; }

    public PSDataFileMapEntry() { }

    public PSDataFileMapEntry(string key, IPSDataFileObject value)
    {
        Key = key;
        if (!TrySetValue(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }
    }

    public IPSDataFileObject GetValue() => _value;

    public bool TrySetValue(IPSDataFileObject value)
    {
        if (CanSetValue(value))
        {
            _value = value;
            return true;
        }

        return false;
    }

    private static bool CanSetValue(IPSDataFileObject value) => value is not PSDataFileMapEntry;
}