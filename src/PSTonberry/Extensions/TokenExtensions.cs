using System.Management.Automation.Language;
using PSTonberry.Model;

namespace PSTonberry;

internal static class TokenExtensions
{
    public static IPSTokenEntry CreateDataToken(this Token token, int index)
    {
        return token switch
        {
            NumberToken numberToken => new PSNumberToken(numberToken, index),
            StringExpandableToken strExToken => new PSStringExpandableToken(strExToken, index),
            StringLiteralToken strLitToken => new PSStringLiteralToken(strLitToken, index),
            VariableToken varToken => new PSVariableToken(varToken, index),
            _ => new PSBasicToken(token, index)
        };
    }
}