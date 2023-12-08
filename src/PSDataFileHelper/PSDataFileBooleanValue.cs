namespace PSDataFileHelper;

public sealed class PSDataFileBooleanValue : IPSDataFileValue<bool?>
{
    private bool? _value;

    public bool HasValue => _value.HasValue;

    public bool IsCollection => false;

    public bool IsReadOnly { get; set; }

    public PSDataFileBooleanValue() { }

    public bool? GetValue() => _value;

    public void SetValue(bool? value) => _value = value;

    public bool TrySetValue(object value)
    {
        switch (value)
        {
            case null:
                _value = null;
                return true;
            case bool booleanValue:
                _value = booleanValue;
                return true;
            case string strValue:
                if (bool.TryParse(strValue, out bool boolResult))
                {
                    _value = boolResult;
                    return true;
                }

                return false;
            default:
                return false;
        }
    }
}