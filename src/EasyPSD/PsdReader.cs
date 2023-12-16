using System;
using System.Collections.Generic;
using System.Management.Automation.Language;

namespace EasyPSD;

public sealed class PsdReader
{
    private PsdFile _dataFile;

    List<Exception> _errors = new List<Exception>();

    private Stack<PsdBaseMap> _maps;

    public bool HasErrors => _errors is not null && _errors.Count > 0;

    internal TokenManager Token { get; set; }

    public PsdReader() { }

    public PsdReaderResult ReadFile(string filePath)
    {
        Token = new TokenManager();
        if (Token.TryGetDataFile(filePath, out string fileContent, out Exception error))
        {
            _dataFile = new PsdFile(filePath, fileContent);
            _maps = new Stack<PsdBaseMap>();
            _maps.Push(_dataFile);
            ReadAllTokens();
            if (HasErrors)
            {
                return new PsdReaderResult(null, _errors, Token.GetRange(0, Token.Index));
            }

            return new PsdReaderResult(_dataFile, _errors);
        }

        return new PsdReaderResult(null, _errors);
    }

    private void ReadAllTokens()
    {
        if (Token.Count <= 0)
        {
            _errors.Add(new PsdReaderException(string.Empty, Token.Index));
            return;
        }
    }

    private IPsdObject ReadArrayExpression()
    {
        throw new NotImplementedException();
    }

    private IPsdObject ReadArrayLiteral()
    {
        throw new NotImplementedException();
    }

    private IPsdObject ReadComment()
    {
        throw new NotImplementedException();
    }

    private IPsdObject ReadMap()
    {
        throw new NotImplementedException();
    }

    private IPsdObject ReadMapEntry()
    {
        throw new NotImplementedException();
    }

    private IPsdObject ReadNumber()
    {
        throw new NotImplementedException();
    }

    private IPsdObject ReadString()
    {
        if (Token.Current is StringToken stringToken)
        {
            if (Token.IsDoubleQuotedString())
            {
                return new DoubleQuotedString((StringToken)Token.Current);
            }

            return new SingleQuotedString((StringToken)Token.Current);
        }

        _errors.Add(new PsdReaderException(nameof(StringToken), Token.Index));
        return null;
    }

    private IPsdObject ReadVariable()
    {
        if (Token.Current is VariableToken variableToken)
        {
            if (variableToken.Name.Equals("true") || variableToken.Name.Equals("false"))
            {
                return new BooleanValue(variableToken);
            }

            return new VariableValue(variableToken);
        }

        _errors.Add(new PsdReaderException(nameof(VariableToken), Token.Index));
        return null;
    }

    private bool TryAddObject(IPsdObject dataFileObject)
    {
        throw new NotImplementedException();
    }
}