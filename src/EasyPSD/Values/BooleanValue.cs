using System.Management.Automation.Language;

namespace EasyPSD;

public sealed class BooleanValue : BaseValue, IPsdCondition, IPsdInlineComment, IPsdValue<bool?>
{
    private bool? _value;

    public CommentValue Comment { get; set; } = null;

    public bool HasInlineComment => Comment is not null;

    public bool HasValue => _value.HasValue;

    public override bool IsCollection => false;

    public override bool IsReadOnly { get; set; }

    public BooleanValue() { }

    internal BooleanValue(VariableToken token) => TrySetValue(token.Name);

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

    string IPsdCondition.GetValue() => GetValue()?.ToString();
}