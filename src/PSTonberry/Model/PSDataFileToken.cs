using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;

namespace PSTonberry.Model;

internal class PSDataFileToken : CollectionBase
{
    public string Identifier => IsIdentifier ? Token.Text : null;

    public int Index { get; internal set; }

    public bool IsComment => Token is not null && Token.Kind is TokenKind.Comment;

    public bool IsModified => Value is not null;

    public bool IsIdentifier => Token is not null && Token.Kind is TokenKind.Identifier;

    public bool IsNewLine => Token is not null && Token.Kind is TokenKind.NewLine;

    public PSDataFileTokenCollection Parent { get; }

    public string Text { get; }

    public Token Token { get; }

    public object Value
    {
        get
        {
            if (Count == 0)
            {
                return null;
            }

            if (Count == 1)
            {
                return List[0];
            }

            var valueArray = Array.CreateInstance(List[0].GetType(), Count);
            List.CopyTo(valueArray, 0);
            return valueArray;
        }
        set
        {
            List.Clear();
            SetValue(value);
        }
    }

    public ArrayList ValuesAdded { get; private set; } = new ArrayList();

    public bool ValuesCleared { get; private set; } = false;

    public ArrayList ValuesRemoved { get; private set; } = new ArrayList();

    public ArrayList ValuesReplaced { get; private set; } = new ArrayList();

    internal bool TrackChanges { get; set; } = false;

    public PSDataFileToken(PSDataFileTokenCollection parent, Token token, int index)
    {
        Parent = parent;
        Token = token;
        Text = token.Text;
        Index = index;
        GetInitialValue();
        TrackChanges = true;
    }

    public void Add(object value) => List.Add(value);

    public void AddRange(object[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] is not null)
            {
                List.Add(values[i]);
            }
        }
    }

    private void GetInitialValue()
    {
        switch (Token)
        {
            case LabelToken labelToken:
                SetValue(labelToken.LabelText);
                break;
            case NumberToken numberToken:
                SetValue(numberToken.Value);
                break;
            case ParameterToken parameterToken:
                SetValue(parameterToken.ParameterName);
                break;
            case StringToken stringToken:
                SetValue(stringToken.Value);
                break;
            case VariableToken variableToken:
                SetValue(variableToken.Name);
                break;
            default:
                SetValue(Token.Text);
                break;
        }
    }

    private void SetValue(object value)
    {
        if (value == null)
        {
            return;
        }

        if (value is Array)
        {
            if (value is byte[])
            {
                Add(value);
            }

            if (value is object[] values)
            {
                AddRange(values);
            }

            object[] valueArray = new object[((Array)value).Length];
            ((Array)value).CopyTo(valueArray, 0);
            AddRange(valueArray);
            return;
        }

        if (value is IList list)
        {
            if (list.Count == 0)
            {
                return;
            }

            if (list.Count == 1)
            {
                Add(list[0]);
                return;
            }

            object[] valueArray = new object[list.Count];
            list.CopyTo(valueArray, 0);
            AddRange(valueArray);
            return;
        }

        List.Add(value);
    }

    protected override void OnClearComplete()
    {
        if (TrackChanges)
        {
            ValuesAdded.Clear();
            ValuesRemoved.Clear();
            ValuesReplaced.Clear();
            ValuesCleared = true;
        }
    }

    protected override void OnInsertComplete(int index, object value)
    {
        if (TrackChanges)
        {
            if (ValuesCleared)
            {
                ValuesReplaced.Add(value);
                ValuesCleared = false;
            }
            else if (ValuesReplaced.Count > 0)
            {
                ValuesReplaced.Add(value);
            }
            else if (ValuesRemoved.Contains(value))
            {
                ValuesRemoved.Remove(value);
            }
            else
            {
                ValuesAdded.Add(value);
            }
        }
    }

    protected override void OnRemoveComplete(int index, object value)
    {
        if (TrackChanges)
        {
            if (ValuesReplaced.Count > 0)
            {
                ValuesReplaced.Remove(value);
            }
            else
            {
                if (ValuesCleared)
                {
                    return;
                }

                if (ValuesAdded.Contains(value))
                {
                    ValuesAdded.Remove(value);
                }
                else
                {
                    ValuesRemoved.Add(value);
                }
            }
        }
    }

    protected override void OnSetComplete(int index, object oldValue, object newValue)
    {
        if (TrackChanges)
        {
            if (ValuesCleared)
            {
                ValuesReplaced.Add(newValue);
                ValuesCleared = false;
            }
            else if (ValuesReplaced.Count > 0)
            {
                ValuesReplaced.Remove(oldValue);
                ValuesReplaced.Add(newValue);
            }
            else
            {
                ValuesRemoved.Add(oldValue);
                ValuesAdded.Remove(oldValue);
                ValuesAdded.Add(newValue);
                ValuesRemoved.Remove(newValue);
            }
        }
    }
}

internal class PSDataFileTokenCollection : ICollection<PSDataFileToken>
{
    private List<PSDataFileToken> _tokens = [];

    public int Count => _tokens.Count;

    public string Identifier => _tokens?.FirstOrDefault(token => token.IsIdentifier)?.Identifier;

    public int Indent { get; }

    public bool IsEmptyLine => Count == 1 && _tokens[0].IsNewLine;

    public bool IsMultiLineComment => Count > 2 && _tokens.All(token => token.IsComment || token.IsNewLine);

    public bool IsReadOnly => false;

    public bool IsSingleLineComment => Count == 2 && _tokens[0].IsComment && _tokens[1].IsNewLine;

    public int Line { get; }

    public HashSet<string> TokensAdded { get; private set; } = [];

    public HashSet<string> TokensModified { get; private set; } = [];

    public HashSet<string> TokensRemoved { get; private set; } = [];

    public object Values
    {
        get
        {
            if (Count > 0)
            {
                var tokens = _tokens.Where(token => Resources.ValueTokens.Contains(token.Token.Kind));
                if (tokens.Any())
                {
                    var valuesArray = new string[tokens.Count()];
                    int count = 0;
                    foreach (var token in tokens)
                    {
                        valuesArray[count] = token.Value.ToString();
                    }

                    return valuesArray;
                }
            }

            return null;
        }
    }

    internal List<PSDataFileToken> Tokens { get; set; }

    internal bool TrackChanges { get; set; } = false;

    public PSDataFileTokenCollection(int line, int indent)
    {
        Line = line;
        Indent = indent;
    }

    public void Add(Token token, int index) => Add(new PSDataFileToken(this, token, index));

    public void Add(PSDataFileToken token)
    {
        _tokens?.Add(token);
        OnInsertComplete(token.Value.ToString());
    }

    public void Clear() => _tokens?.Clear();

    public bool Contains(PSDataFileToken token) => _tokens.Contains(token);

    public void CopyTo(PSDataFileToken[] tokens, int offset) => _tokens?.CopyTo(tokens, offset);

    public IEnumerator<PSDataFileToken> GetEnumerator() => _tokens?.GetEnumerator();

    public bool Remove(PSDataFileToken token)
    {
        var result = _tokens.Remove(token);
        OnRemoveComplete(token.Value.ToString());
        return result;
    }

    internal virtual void OnInsertComplete(string key)
    {
        if (TrackChanges)
        {
            TokensRemoved.Remove(key);
            TokensAdded.Add(key);
        }
    }

    internal virtual void OnRemoveComplete(string key)
    {
        if (TrackChanges)
        {
            TokensRemoved.Add(key);
            TokensAdded.Remove(key);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
