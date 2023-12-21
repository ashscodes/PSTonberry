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
                    _maps.Peek().Add(_psdFile.Data);
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
            TestForArrayClose();
        }

        Tokens.ConsumeToken();
        TestForInlineComment(array);
        return array;
    }

    private PsdArrayLiteral ReadArrayLiteral()
    {
        var array = new PsdArrayLiteral();
        bool isArrayEnd;
        do
        {
            var arrayValue = ReadArrayValue();
            array.Add(arrayValue);
            isArrayEnd = TestForArrayClose();
        }
        while (!isArrayEnd);

        TestForInlineComment(array);
        return array;
    }

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
            value = ReadKeywordBlock();
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

    private PsdKeyword ReadKeywordBlock()
    {
        PsdKeyword keywordBlock = new(Tokens.Current.Text);
        Tokens.ConsumeToken();
        if (Tokens.Current.IsNewLine())
        {
            Tokens.ConsumeToken();
        }

        if (Tokens.Current.IsArrayStart())
        {
            keywordBlock.Condition = ReadKeywordConditions();
        }
        else if (Tokens.Current.Is(TokenKind.LCurly))
        {
            keywordBlock.Scriptblock = ReadKeywordScriptblock();
        }

        return keywordBlock;
    }

    private PsdConditionCollection ReadKeywordConditions()
    {
        var initialCondition = new PsdConditionCollection(Tokens.Current);
        var conditions = new Stack<PsdConditionCollection>();
        conditions.Push(initialCondition);
        Tokens.ConsumeToken();

        while (conditions.Count != 0)
        {
            if (Tokens.Current.IsArrayStart())
            {
                conditions.Push(new PsdConditionCollection(Tokens.Current));
            }
            else if (Tokens.Current.IsArrayClose())
            {
                var condition = conditions.Pop();
                TestForInlineComment(condition);
            }
            else if (Tokens.Current.IsNewLine() || Tokens.Current.Is(TokenKind.Comma))
            {
                // Just consume the token
            }
            else
            {
                var simpleValue = ReadSimpleValue();
                if (simpleValue is IPsdCondition conditionObject)
                {
                    conditions.Peek().Add(conditionObject);
                }
            }

            Tokens.ConsumeToken();
        }

        return initialCondition;
    }

    private PsdScriptblock ReadKeywordScriptblock()
    {
        int nestedScriptBlocks = 0;
        var scriptblock = new PsdScriptblock();
        Tokens.ConsumeToken();
        Tokens.InScriptblock = true;
        while (Tokens.InScriptblock)
        {
            if (Tokens.Current.IsScriptblockStart())
            {
                nestedScriptBlocks++;
            }

            if (Tokens.Current.IsMapClose())
            {
                nestedScriptBlocks--;
                if (nestedScriptBlocks == 0)
                {
                    Tokens.InScriptblock = false;
                }
            }

            if (!Tokens.Current.IsNewLine())
            {
                scriptblock.Add(ReadSimpleValue());
            }

            Tokens.ConsumeToken();
        }

        TestForInlineComment(scriptblock);
        return scriptblock;
    }

    private IPsdObject ReadSimpleValue()
    {
        var value = BaseValue.Create(Tokens.Current);
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

    private bool TestForArrayClose()
    {
        if (Tokens.Current.Is(TokenKind.Comma))
        {
            Tokens.ConsumeToken();
        }

        if (Tokens.Current.IsNewLine())
        {
            Tokens.ConsumeToken();
        }

        if (Tokens.IsSimpleValue()
            || Tokens.Current.IsComment()
            || Tokens.Current.IsArrayStart()
            || Tokens.Current.IsMapStart())
        {
            return false;
        }

        return true;
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
            _maps.Peek().Add(psdObject);

            if (psdObject is PsdMapEntry mapEntry && mapEntry.GetValue() is PsdNamedMap namedMap)
            {
                _maps.Peek().NamedMaps.Add(namedMap);
                _maps.Push(namedMap);
            }
        }
    }
}