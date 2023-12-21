using System.Management.Automation.Language;

namespace EasyPSD;

public abstract class BaseValue : IPsdObject
{
    public abstract bool IsCollection { get; }

    public abstract bool IsReadOnly { get; set; }

    public static IPsdObject Create(Token token)
    {
        return token switch
        {
            NumberToken numberToken => new NumberValue(numberToken),
            StringToken stringToken => BaseStringValue.Create(stringToken),
            VariableToken variableToken when variableToken.Name.Equals("true") => new BooleanValue(variableToken),
            VariableToken variableToken when variableToken.Name.Equals("false") => new BooleanValue(variableToken),
            VariableToken variableToken => new VariableValue(variableToken),
            Token when token.Is(TokenFlags.BinaryPrecedenceComparison) => new ComparisonCondition(token.Text),
            Token when token.Is(TokenFlags.BinaryPrecedenceLogical) => new LogicalCondition(token.Text),
            _ => new ScriptblockValue(token.Text)
        };
    }
}