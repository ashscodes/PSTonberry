using System;
using System.Management.Automation.Language;

namespace PSTonberry.Model;

internal abstract class PSTokenEntry<T, U> : PSTokenEntry where U : Token
{
    private readonly T _initialValue;

    private T _value;

    public U PSDataToken { get; }

    public override TokenKind Kind => PSDataToken.Kind;

    internal T Value
    {
        get => _value;
    }

    internal abstract Func<T, int> Compare { get; }

    internal PSTokenEntry(U token, int index, T value)
    {
        Index = index;
        PSDataToken = token;
        Text = token.Text;
        _initialValue = value;
        TrackChanges = true;
    }

    public override Token GetPSToken() => PSDataToken;

    public override string GetValue()
    {
        _value ??= _initialValue;
        return _value switch
        {
            string strValue => strValue,
            object => _value.ToString(),
            _ => string.Empty
        };
    }

    public void SetValue(T value)
    {
        if (!IsReadOnly)
        {
            _value = value;
        }
    }

    internal override int CompareValue() => Compare(_initialValue);
}