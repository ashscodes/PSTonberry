using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;

namespace PSDataFileHelper;

public sealed class PSDataFileReader
{
    private PSDataFile _dataFile;

    private List<Token> _tokenList;

    internal int ArrayLevel { get; set; } = 0;

    internal int ConsecutiveNewLines { get; set; } = 0;

    internal bool IsInArray { get; set; } = false;

    internal bool IsInCollection => IsInArray || IsInMap;

    internal bool IsInCoreObject { get; set; } = false;

    internal bool IsInMap { get; set; } = false;

    internal int MapLevel { get; set; } = 0;

    public PSDataFileReader() { }

    public PSDataFile ReadFile(string filePath)
    {
        _dataFile = new PSDataFile(filePath);
        if (TryParseDataFileString(filePath, out string fileContent, out Token[] tokens, out Exception exception))
        {
            _dataFile.OriginalText = fileContent;

            string privateData;
            string psData;
            if (fileContent.TryGetDataSection(nameof(PrivateData), out privateData))
            {
                _dataFile.PrivateData = new PrivateData(privateData);
                if (fileContent.TryGetDataSection(nameof(PSData), out psData))
                {
                    _dataFile.PrivateData.PSData = new PSData(psData);
                }
            }

            _tokenList = new List<Token>(tokens);
            ProcessTokens();
            return _dataFile;
        }

        throw exception;
    }

    private int GetNextNewLine(int startIndex)
    {
        if (_tokenList is null || _tokenList.Count == 0)
        {
            return -1;
        }

        Ensure.IndexIsInRange(startIndex, _tokenList.Count);

        for (; startIndex < _tokenList.Count; startIndex++)
        {
            if (_tokenList[startIndex].Kind == TokenKind.NewLine)
            {
                return startIndex;
            }
        }

        return -1;
    }

    private bool IsValueArrayExpression(int index)
    {
        bool isFirstTokenAssignment = _tokenList[index + 1].Kind is TokenKind.Equals;
        bool isSecondTokenArrayOpen = _tokenList[index + 2].Kind is TokenKind.AtParen;
        return isFirstTokenAssignment && isSecondTokenArrayOpen;
    }

    private bool IsValueArrayLiteral(int index)
    {
        bool isFirstTokenAssignment = _tokenList[index + 1].Kind is TokenKind.Equals;
        bool isSecondTokenSimpleValue = Resources.ValueTokens.Contains(_tokenList[index + 2].Kind);
        bool isThirdTokenComma = _tokenList[index + 3].Kind is TokenKind.Comma;

        return isFirstTokenAssignment && isSecondTokenSimpleValue && isThirdTokenComma;
    }

    private bool IsValueEmptyArrayExpression(int index)
    {
        bool isArrayValue = IsValueArrayExpression(index);
        bool isSecondTokenArrayClose = _tokenList[index + 2].Kind is TokenKind.RParen;
        return isArrayValue && isSecondTokenArrayClose;
    }

    private bool IsValueEmptyMap(int index)
    {
        bool isMapValue = IsValueMap(index);
        bool isSecondTokenMapClose = _tokenList[index + 2].Kind is TokenKind.RCurly;
        return isMapValue && isSecondTokenMapClose;
    }

    private bool IsValueLastArrayItem(int index)
        => _tokenList[index + 1].Kind is not TokenKind.Comma || _tokenList[index + 1].Kind is not TokenKind.Semi;

    private bool IsValueMap(int index)
    {
        bool isFirstTokenAssignment = _tokenList[index + 1].Kind is TokenKind.Equals;
        bool isSecondTokenMapOpen = _tokenList[index + 2].Kind is TokenKind.AtCurly;
        return isFirstTokenAssignment && isSecondTokenMapOpen;
    }

    private bool IsValueSimplePair(int index)
    {
        bool isFirstTokenAssignment = _tokenList[index + 1].Kind is TokenKind.Equals;
        bool isSecondTokenSimpleValue = Resources.ValueTokens.Contains(_tokenList[index + 2].Kind);
        bool isThirdTokenNewLine = _tokenList[index + 3].Kind is TokenKind.NewLine
            || _tokenList[index + 3].Kind is TokenKind.Semi;

        return isFirstTokenAssignment && isSecondTokenSimpleValue && isThirdTokenNewLine;
    }

    private void ProcessComment(int index)
    {
        var comment = new PSDataFileComment();
        comment.SetValue(_tokenList[index].Text);
        comment.HasPrecedingEmptyLine = ConsecutiveNewLines >= 2;
        _dataFile.Add(comment);
    }

    private void ProcessNewLine()
    {
        if (IsInCollection)
        {
            return;
        }

        ConsecutiveNewLines++;
    }

    private void ProcessTokens()
    {
        if (_tokenList.Count <= 0)
        {
            return;
        }

        // TokenKind determines the value of i.
        for (int i = 0; i < _tokenList.Count;)
        {
            if (_tokenList[i].Kind is TokenKind.Comment)
            {
                ProcessComment(i);
                ConsecutiveNewLines = 0;
                i++;
            }
            else if (_tokenList[i].Kind is TokenKind.NewLine)
            {
                ProcessNewLine();
                i++;
            }
            else if (_tokenList[i].Kind is TokenKind.AtCurly)
            {
                _dataFile.AdditionalData.Add(new PSDataFileCoreObject());
                IsInCoreObject = true;
            }
            else if (_tokenList[i].Kind is TokenKind.RCurly)
            {
                IsInCoreObject = false;
            }
            else
            {
                ReadItem(ref i);
                ConsecutiveNewLines = 0;
            }
        }
    }

    private void ReadItem(ref int index)
    {
        int nextLineBreak = GetNextNewLine(index);
        if (TryGetIdentifier(index, out PSDataFileMapEntry mapEntry))
        {
            if (mapEntry.Key.Equals(nameof(PrivateData)))
            {
                TryReadPrivateData(ref index);
                return;
            }

            if (mapEntry.Key.Equals(nameof(PSData)))
            {
                TryReadPSData(ref index);
                return;
            }

            IPSDataFileObject mapValue = null;
            if (IsValueEmptyArrayExpression(index))
            {
                TryReadEmptyArray(ref index, out mapValue);
            }
            else if (IsValueArrayExpression(index))
            {
                TryReadArrayExpression(ref index, out mapValue);
            }
            else if (IsValueArrayLiteral(index))
            {
                TryReadArrayLiteral(ref index, out mapValue);
            }
            else if (IsValueEmptyMap(index))
            {
                TryReadEmptyMap(ref index, out mapValue);
            }
            else if (IsValueMap(index))
            {
                TryReadMap(ref index, out mapValue);
            }
            else
            {
                if (IsValueSimplePair(index))
                {
                    TryReadSimpleMapEntry(ref index, out mapValue);
                }
            }

            if (mapValue is not null)
            {
                mapEntry.TrySetValue(mapValue);
                if (IsInCoreObject)
                {
                    _dataFile.Add(mapEntry);
                }
                else
                {
                    _dataFile.AdditionalData.Add(mapEntry);
                }
            }
        }
    }

    private bool TryGetIdentifier(int index, out PSDataFileMapEntry mapEntry)
    {
        mapEntry = new PSDataFileMapEntry();
        if (_tokenList[index].Kind is TokenKind.Identifier)
        {
            mapEntry.Key = _tokenList[index].Text;
            return true;
        }

        if (_tokenList[index] is StringToken stringToken)
        {
            mapEntry.Key = stringToken.Value;
            return true;
        }

        return false;
    }

    private static bool TryParseDataFileString(string filePath,
                                               out string fileContent,
                                               out Token[] tokens,
                                               out Exception exception)
    {
        exception = null;
        fileContent = string.Empty;
        var scriptblock = Parser.ParseFile(filePath, out tokens, out ParseError[] errors);
        if (errors.Length > 0)
        {
            exception = new InvalidOperationException(string.Format(Resources.DataFileInvalid, filePath));
            return false;
        }

        fileContent = scriptblock.ToString();
        return true;
    }

    private void TryReadPrivateData(ref int index)
    {
        throw new NotImplementedException();
    }

    private void TryReadPSData(ref int index)
    {
        throw new NotImplementedException();
    }

    private static bool TryReadArrayExpression(ref int index, out IPSDataFileObject mapValue)
    {
        throw new NotImplementedException();
    }

    private static bool TryReadArrayLiteral(ref int index, out IPSDataFileObject mapValue)
    {
        throw new NotImplementedException();
    }

    private static bool TryReadEmptyArray(ref int index, out IPSDataFileObject mapValue)
    {
        mapValue = new PSDataFileArrayExpression();
        index += 4;
        return true;
    }

    private static bool TryReadEmptyMap(ref int index, out IPSDataFileObject mapValue)
    {
        mapValue = new PSDataFileNestedMap();
        index += 4;
        return true;
    }

    private static bool TryReadMap(ref int index, out IPSDataFileObject mapValue)
    {
        throw new NotImplementedException();
    }

    private static bool TryReadSimpleMapEntry(ref int index, out IPSDataFileObject mapValue)
    {
        throw new NotImplementedException();
    }
}