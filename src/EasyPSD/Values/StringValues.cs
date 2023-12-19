using System;
using System.Management.Automation.Language;

namespace EasyPSD;

public sealed class CommentValue : BaseStringValue, IPsdValueSpacing
{
    public bool HasPrecedingEmptyLine { get; set; }

    public bool IsEmptyComment => GetValue() == "#";

    public bool IsInMap { get; set; }

    public bool IsMultiLine => GetValue().StartsWith("<#");

    public CommentValue(string value, bool hasPrecedingEmptyLine = false) : base(value)
    {
        HasPrecedingEmptyLine = hasPrecedingEmptyLine;
    }

    internal CommentValue(StringToken token) : base(token) { }

    public bool TryGetMapEntry(out PsdMapEntry mapEntry)
    {
        mapEntry = null;
        return true;
    }
}

public sealed class ScriptblockValue : BaseStringValue
{
    public ScriptblockValue(string value) : base(value) { }

    internal ScriptblockValue(StringToken token) : base(token) { }
}

public sealed class DoubleQuotedString : BaseStringValue
{
    public DoubleQuotedString(string value) : base(value) { }

    public DoubleQuotedString(StringToken token) : base(token) { }

    public override string ToString()
    {
        if (IsHereString)
        {
            return "@\"" + GetValue() + "\"";
        }

        return "\"" + GetValue() + "\"";
    }
}

public sealed class SingleQuotedString : BaseStringValue
{
    public SingleQuotedString(string value) : base(value) { }

    internal SingleQuotedString(StringToken token) : base(token) { }

    public override string ToString()
    {
        if (IsHereString)
        {
            return "@'" + GetValue() + "'";
        }

        return "'" + GetValue() + "'";
    }
}

public sealed class VariableValue : BaseStringValue
{
    public bool IsSplattedVariable { get; set; } = false;

    public string VariablePath { get; set; }

    public VariableValue(string value) : base(value) { }

    internal VariableValue(VariableToken token) : base(token.Name)
    {
        IsSplattedVariable = token.Kind is TokenKind.SplattedVariable;
    }

    public override string ToString()
    {
        if (IsSplattedVariable)
        {
            return '@' + GetValue();
        }

        return '$' + GetValue();
    }
}

public abstract class BaseStringValue : IPsdInlineComment, IPsdValue<string>
{
    private string _value;

    public CommentValue Comment { get; set; } = null;

    public bool HasInlineComment => Comment is not null;

    public bool HasValue => !string.IsNullOrEmpty(_value);

    public bool IsCollection => false;

    public bool IsHereString { get; set; } = false;

    public bool IsReadOnly { get; internal set; } = false;

    public BaseStringValue(string value)
    {
        if (!TrySetValue(value))
        {
            throw new ArgumentException(string.Format(Resources.CouldNotSetValue, typeof(string)), nameof(value));
        }
    }

    protected BaseStringValue(StringToken token)
    {
        if (!TrySetValue(token.Value))
        {
            throw new ArgumentException(string.Format(Resources.CouldNotSetValue, typeof(string)), nameof(token));
        }

        IsHereString = token.Kind is TokenKind.HereStringExpandable
            || token.Kind is TokenKind.HereStringLiteral;
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

    public override string ToString() => GetValue();

    public static BaseStringValue Create(StringToken token)
    {
        if (token.Kind == TokenKind.HereStringExpandable || token.Kind == TokenKind.StringExpandable)
        {
            return new DoubleQuotedString(token);
        }
        else
        {
            return new SingleQuotedString(token);
        }
    }
}