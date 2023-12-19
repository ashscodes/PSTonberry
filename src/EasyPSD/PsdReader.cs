using System;
using System.Collections.Generic;
using System.Management.Automation.Language;

namespace EasyPSD;

public sealed class PsdReader
{
    private List<Exception> _errors = [];

    private Stack<PsdBaseMap> _maps;

    private PsdFile _psdFile;

    public bool HasErrors => _errors is not null && _errors.Count > 0;

    internal TokenManager Tokens { get; set; }

    public PsdReader() { }

    public PsdReaderResult ReadFile(string filePath)
    {
        if (_errors.Count > 0)
        {
            _errors.Clear();
        }

        Tokens = new TokenManager();
        if (Tokens.TryGetDataFile(filePath, out string fileContent, out Exception error))
        {
            _psdFile = new PsdFile(filePath, fileContent);
            _maps = new Stack<PsdBaseMap>();
            _maps.Push(_psdFile);
            ReadAllTokens();
            if (HasErrors)
            {
                return new PsdReaderResult(null, _errors, Tokens.GetRange(0, Tokens.Index));
            }

            return new PsdReaderResult(_psdFile, _errors);
        }

        return new PsdReaderResult(null, _errors);
    }

    private PsdNamedMap NewNamedMap(string identifier)
    {
        var namedMap = new PsdNamedMap(identifier);
        Tokens.ConsumeToken();
        return namedMap;
    }

    private void ReadAllTokens()
    {
        if (Tokens.Count <= 0)
        {
            _errors.Add(new PsdReaderException(string.Empty, Tokens.Index));
            return;
        }

        // Process items as complete object
        while (Tokens.Index != Tokens.Count - 1)
        {
            if (Tokens.Current.IsMapStart() && Tokens.HasPrecedingNewLine)
            {
                if (_maps is not null && _maps.Count == 1 && _maps.Peek() is PsdFile)
                {
                    _maps.Push(_psdFile.Data);
                    Tokens.ConsumeToken();
                }
            }
            else if (Tokens.Current.IsMapClose())
            {
                if (_maps is not null)
                {
                    var map = _maps.Pop();
                    Tokens.ConsumeToken();
                    TestForInlineComment(map);
                }
            }
            else
            {
                try
                {
                    UpdateMap();
                }
                catch (Exception ex)
                {
                    _errors.Add(ex);
                    return;
                }
            }
        }
    }

    private PsdArrayExpression ReadArrayExpression()
    {
        var array = new PsdArrayExpression();
        array.HasAtSymbol = Tokens.Current.Is(TokenKind.AtParen);
        Tokens.ConsumeToken();
        if (Tokens.Current.Is(TokenKind.RParen))
        {
            return array;
        }

        while (!Tokens.Current.IsArrayClose())
        {
            var arrayValue = ReadArrayValue();
            array.Add(arrayValue);
        }

        TestForInlineComment(array);
        return array;
    }

    private PsdArrayLiteral ReadArrayLiteral()
    {
        var array = new PsdArrayLiteral();
        Tokens.ConsumeToken();
        do
        {
            var arrayValue = ReadArrayValue();
            array.Add(arrayValue);
            if (Tokens.Current.Is(TokenKind.Comma))
            {
                Tokens.ConsumeToken();
            }
        } while ((Tokens.Current.Is(TokenKind.NewLine) && Tokens.Previous.IsNot(TokenKind.Comma))
            || Tokens.Next.IsNot(TokenKind.Comma));

        TestForInlineComment(array);
        return array;
    }

    // Need to investigate flow of values returned.
    private IPsdObject ReadArrayValue()
    {
        IPsdObject value = null;
        if (Tokens.Current.IsComment())
        {
            value = ReadComment();
        }
        else if (Tokens.IsSimpleValue())
        {
            value = ReadSimpleValue();
        }
        else if (Tokens.Current.IsMapStart())
        {
            value = ReadNestedMap();
        }
        else if (Tokens.Current.IsArrayStart())
        {
            value = ReadArrayExpression();
        }
        else
        {
            Tokens.ConsumeToken();
        }

        return value;
    }

    private CommentValue ReadComment()
    {
        if (Tokens.Current.Is(TokenKind.Comma))
        {
            Tokens.ConsumeToken();
        }

        var comment = new CommentValue(Tokens.Current.Text, (Tokens.ConsecutiveNewLines >= 2));
        Tokens.ConsumeToken();
        return comment;
    }

    private string ReadIdentifier()
    {
        string identifier = string.Empty;
        if (Tokens.Current is StringToken stringToken)
        {
            identifier = stringToken.Value;
        }
        else
        {
            identifier = Tokens.Current.Text;
        }

        Tokens.ConsumeToken();
        return identifier;
    }

    private PsdMapEntry ReadMapEntry()
    {
        string identifier = ReadIdentifier();
        Tokens.ConsumeToken();                                      // Consume Assignment operator.
        IPsdObject value = null;
        if (Tokens.Current.IsArrayStart())
        {
            Tokens.ConsumeToken();
            value = ReadArrayExpression();
        }
        else if (Tokens.IsArrayLiteral())
        {
            value = ReadArrayLiteral();
        }
        else if (Tokens.Current.IsMapStart())
        {
            value = NewNamedMap(identifier);
        }
        else if (Tokens.Current.IsKeyword())
        {
            value = ReadScriptblock();
        }
        else if (Tokens.IsSimpleValue())
        {
            value = ReadSimpleValue();
        }
        else
        {
            throw new PsdReaderException(identifier, Tokens.Index);
        }

        return new PsdMapEntry(identifier, value);
    }

    private IPsdObject ReadNestedMap()
    {
        var nestedMap = new PsdNestedMap();
        Tokens.ConsumeToken();
        while (!Tokens.Current.IsMapClose())
        {
            var psdObject = ReadPsdObject();
            if (psdObject is not null)
            {
                nestedMap.Add(psdObject);
            }
        }

        TestForInlineComment(nestedMap);
        return nestedMap;
    }

    private NumberValue ReadNumber(NumberToken numberToken) => new NumberValue(numberToken);

    // Need to see if we should check for NewLine here.
    private IPsdObject ReadPsdObject()
    {
        IPsdObject value = null;
        if (Tokens.Current.IsComment())
        {
            value = ReadComment();
        }
        else if (Tokens.IsMapEntry())
        {
            value = ReadMapEntry();
        }
        else
        {
            Tokens.ConsumeToken();
        }

        return value;
    }

    private PsdScriptblock ReadScriptblock()
    {
        throw new NotImplementedException();
    }

    private IPsdObject ReadSimpleValue()
    {
        IPsdObject value = Tokens.Current switch
        {
            NumberToken numberToken => ReadNumber(numberToken),
            StringToken stringToken => ReadString(stringToken),
            VariableToken variableToken => ReadVariable(variableToken),
            _ => throw new PsdReaderException(Tokens.Current.Text, Tokens.Index)
        };

        Tokens.ConsumeToken();
        TestForInlineComment(value);
        return value;
    }

    private BaseStringValue ReadString(StringToken stringToken) => BaseStringValue.Create(stringToken);

    private IPsdObject ReadVariable(VariableToken variableToken)
    {
        if (variableToken.Name.Equals("true") || variableToken.Name.Equals("false"))
        {
            return new BooleanValue(variableToken);
        }

        return new VariableValue(variableToken);
    }

    private void TestForInlineComment(IPsdObject value)
    {
        if (Tokens.IsInlineComment() && value is IPsdInlineComment inlineComment)
        {
            inlineComment.Comment = ReadComment();
        }
    }

    private void UpdateMap()
    {
        var psdObject = ReadPsdObject();
        if (psdObject is not null)
        {
            if (psdObject is PsdMapEntry mapEntry && mapEntry.GetValue() is PsdNamedMap namedMap)
            {
                _maps.Peek().NamedMaps.Add(namedMap);
                _maps.Push(namedMap);
            }
            else
            {
                var currentMap = _maps.Peek();
                currentMap.Add(psdObject);
            }
        }
    }
}