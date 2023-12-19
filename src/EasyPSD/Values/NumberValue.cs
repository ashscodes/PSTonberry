using System.Management.Automation.Language;

namespace EasyPSD;

public sealed class NumberValue : IPsdInlineComment, IPsdValue<decimal?>
{
    private decimal? _value;

    public CommentValue Comment { get; set; } = null;

    public bool HasInlineComment => Comment is not null;

    public bool HasValue => _value.HasValue;

    public bool IsCollection => false;

    public bool IsReadOnly { get; set; }

    public NumberValue() { }

    internal NumberValue(NumberToken token) => TrySetValue(token.Value);

    public decimal? GetValue() => _value;

    public void SetValue(decimal? value) => _value = value;

    public void SetValue(double? value) => _value = (decimal?)value;

    public void SetValue(float? value) => _value = (decimal?)value;

    public void SetValue(int? value) => _value = value;

    public void SetValue(long? value) => _value = value;

    public bool TrySetValue(object value)
    {
        switch (value)
        {
            case null:
                _value = null;
                return true;
            case decimal decimalValue:
            case double doubleValue:
            case float floatValue:
            case int intValue:
            case long longValue:
                _value = (decimal?)value;
                return true;
            case string strValue:
                if (decimal.TryParse(strValue, out decimal decimalResult))
                {
                    _value = decimalResult;
                    return true;
                }

                return false;
            default:
                return false;
        }
    }
}