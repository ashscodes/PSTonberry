using System;
using System.Management.Automation.Language;

namespace PSTonberry.Model;

internal class PSBasicToken : PSStringToken<Token>
{
    internal override Func<string, int> Compare => str => { return 0; };

    public PSBasicToken(Token token, int index) : base(token, index, token.Text)
    {
        IsReadOnly = true;
    }

    public override string ToString() => Text;
}

internal class PSNumberToken : PSTokenEntry<double, NumberToken>
{
    internal override Func<double, int> Compare => number => Value.CompareTo(number);

    public PSNumberToken(NumberToken token, int index) : base(token, index, (double)token.Value) { }

    public override string ToString() => Value.ToString();
}

internal class PSStringExpandableToken : PSStringToken<StringExpandableToken>
{
    public PSStringExpandableToken(StringExpandableToken token, int index) : base(token, index, token.Value) { }

    public override string ToString() => "\"" + Value + "\"";
}

internal class PSStringLiteralToken : PSStringToken<StringLiteralToken>
{
    public PSStringLiteralToken(StringLiteralToken token, int index) : base(token, index, token.Value) { }

    public override string ToString() => "'" + Value + "'";
}

internal class PSVariableToken : PSStringToken<VariableToken>
{
    public PSVariableToken(VariableToken token, int index) : base(token, index, token.Name) { }

    public override string ToString() => "$" + Value;
}

internal abstract class PSStringToken<U> : PSTokenEntry<string, U> where U : Token
{
    internal override Func<string, int> Compare => str => Value.CompareTo(str);

    public PSStringToken(U token, int index, string value) : base(token, index, value) { }
}