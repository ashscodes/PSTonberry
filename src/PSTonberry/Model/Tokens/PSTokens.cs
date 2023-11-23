using System;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace PSTonberry.Model;

internal class PSBasicToken : PSStringToken
{
    internal override Func<string, int> Compare => str => { return 0; };

    public PSBasicToken() { }

    public PSBasicToken(Token token, int index) : base(token, index, token.Text)
    {
        IsReadOnly = true;
    }

    public override string ToString() => Text;

    public override PSBasicToken Clone() => base.Clone<PSBasicToken>();
}

internal class PSNumberToken : PSTokenEntry<decimal>, IClonablePSToken<PSNumberToken>
{
    public decimal Value { get; set; }

    internal override Func<decimal, int> Compare => number => Value.CompareTo(number);

    public PSNumberToken() { }

    public PSNumberToken(NumberToken token, int index) : base(token, index, (decimal)token.Value) { }

    public PSNumberToken Clone()
    {
        var token = base.Clone<PSNumberToken>();
        token.Value = Value;
        return token;
    }

    public override object GetValue() => Value;

    public override void SetValue(decimal value) => Value = value;

    public override string ToString() => Value.ToString();
}

internal class PSStringExpandableToken : PSStringToken
{
    public PSStringExpandableToken() { }

    public PSStringExpandableToken(StringExpandableToken token, int index) : base(token, index, token.Value) { }

    public override PSStringExpandableToken Clone()
    {
        var token = base.Clone<PSStringExpandableToken>();
        token.Value = Value;
        return token;
    }

    public override string ToString() => "\"" + Value + "\"";
}

internal class PSStringLiteralToken : PSStringToken
{
    public PSStringLiteralToken() { }

    public PSStringLiteralToken(StringLiteralToken token, int index) : base(token, index, token.Value) { }

    public override PSStringLiteralToken Clone()
    {
        var token = base.Clone<PSStringLiteralToken>();
        token.Value = Value;
        return token;
    }

    public override string ToString() => "'" + Value + "'";
}

internal class PSVariableToken : PSTokenEntry<string>, IClonablePSToken<PSVariableToken>
{
    public string Name { get; set; }

    public VariablePath Path { get; set; }

    internal override Func<string, int> Compare => str => Name.CompareTo(str);

    public PSVariableToken() { }

    public PSVariableToken(VariableToken token, int index) : base(token, index, token.Name)
    {
        Name = token.Name;
        Path = token.VariablePath;
    }

    public PSVariableToken Clone()
    {
        var token = base.Clone<PSVariableToken>();
        token.Name = Name;
        token.Path = Path;
        return token;
    }

    public override object GetValue() => Name;

    public override void SetValue(string value) => Name = value;

    public override string ToString() => "$" + Name;
}

internal abstract class PSStringToken : PSTokenEntry<string>, IClonablePSToken<PSStringToken>
{
    public string Value { get; set; }

    internal override Func<string, int> Compare => str => Value.CompareTo(str);

    public PSStringToken() { }

    public PSStringToken(Token token, int index, string value) : base(token, index, value) { }

    public override object GetValue() => Value;

    public override void SetValue(string value) => Value = value;

    public abstract PSStringToken Clone();
}