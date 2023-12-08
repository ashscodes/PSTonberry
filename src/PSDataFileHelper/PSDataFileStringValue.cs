using System;

namespace PSDataFileHelper;

public sealed class PSDataFileComment : PSDataFileStringValue, IPSDataFileLine
{
    public bool HasPrecedingEmptyLine { get; set; }

    public bool IsEmptyComment => GetValue() == "#";

    public bool IsInMap { get; set; }

    public bool IsMultiLine => GetValue().StartsWith("<#");

    public PSDataFileComment() { }

    public PSDataFileComment(string value) : base(value) { }

    public bool TryGetMapEntry(out PSDataFileMapEntry mapEntry)
    {
        mapEntry = null;
        return true;
    }

    public override string ToString() => GetValue();
}

public sealed class PSDataFileScriptblockValue : PSDataFileStringValue
{
    public PSDataFileScriptblockValue() => IsHereString = false;

    public PSDataFileScriptblockValue(string value) : base(value) => IsHereString = false;

    public override string ToString() => GetValue();
}

public sealed class PSDataFileStringExpandableValue : PSDataFileStringValue
{
    public PSDataFileStringExpandableValue() { }

    public PSDataFileStringExpandableValue(string value) : base(value) { }

    public override string ToString()
    {
        if (IsHereString)
        {
            return "@\"" + GetValue() + "\"";
        }

        return "\"" + GetValue() + "\"";
    }
}

public sealed class PSDataFileStringLiteralValue : PSDataFileStringValue
{
    public PSDataFileStringLiteralValue() { }

    public PSDataFileStringLiteralValue(string value) : base(value) { }

    public override string ToString()
    {
        if (IsHereString)
        {
            return "@'" + GetValue() + "'";
        }

        return "'" + GetValue() + "'";
    }
}

public sealed class PSDataFileVariableValue : PSDataFileStringValue
{
    public bool IsSplattedVariable { get; set; } = false;

    public string VariablePath { get; set; }

    public PSDataFileVariableValue() => IsHereString = false;

    public PSDataFileVariableValue(string value) : base(value) => IsHereString = false;

    public override string ToString()
    {
        if (IsSplattedVariable)
        {
            return '@' + GetValue();
        }

        return '$' + GetValue();
    }
}

public abstract class PSDataFileStringValue : IPSDataFileValue<string>
{
    private string _value;

    public bool HasValue => !string.IsNullOrEmpty(_value);

    public bool IsCollection => false;

    public bool IsHereString { get; set; }

    public bool IsReadOnly { get; set; }

    public PSDataFileStringValue() { }

    public PSDataFileStringValue(string value)
    {
        if (!TrySetValue(value))
        {
            throw new ArgumentException(string.Format(Resources.CouldNotSetValue, typeof(string)), nameof(value));
        }
    }

    public virtual string GetValue() => _value;

    public virtual void SetValue(string value) => _value = value;

    public virtual bool TrySetValue(object value)
    {
        switch (value)
        {
            case null:
                _value = null;
                break;
            case string strValue:
                _value = strValue;
                break;
            default:
                _value = value.ToString();
                break;
        }

        return true;
    }

    public abstract override string ToString();
}