using System;

namespace EasyPSD;

public class PsdMapEntry : IPsdValueSpacing
{
    private IPsdObject _value;

    public string Key { get; set; }

    public bool IsCollection => typeof(IPsdCollection<>).IsAssignableFrom(_value.GetType());

    public bool IsReadOnly { get; set; }

    public bool HasPrecedingEmptyLine { get; set; }

    public PsdMapEntry() { }

    public PsdMapEntry(string key, IPsdObject value)
    {
        Key = key;
        if (!TrySetValue(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }
    }

    public IPsdObject GetValue() => _value;

    public bool TrySetValue(IPsdObject value)
    {
        if (CanSetValue(value))
        {
            _value = value;
            return true;
        }

        return false;
    }

    private static bool CanSetValue(IPsdObject value) => value is not PsdMapEntry;
}