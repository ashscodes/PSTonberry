using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PSTonberry.Model;

internal class PSTokenizedLine : ICollection<PSTokenEntry>
{
    private List<PSTokenEntry> _tokens = [];

    public int Count => _tokens is not null ? _tokens.Count : 0;

    public bool HasIdentifier => Count > 0 && _tokens.Any(t => t.IsIdentifier);

    public string Identifier => _tokens?.FirstOrDefault(t => t.IsIdentifier)?.Text;

    public int Indent { get; }

    public bool IsComment => IsMultiLineComment || IsSingleLineComment;

    public bool IsEmptyLine => Count == 1 && _tokens[0].IsNewLine;

    public bool IsModified => (bool)(_tokens?.Any(t => t.IsModified));

    public bool IsMultiLineComment => Count > 2 && _tokens.All(t => t.IsComment || t.IsNewLine);

    public bool IsReadOnly => false;

    public bool IsSingleLineComment => Count == 2 && _tokens[0].IsComment && _tokens[1].IsNewLine;

    public int Line { get; }

    public HashSet<PSTokenEntry> TokensAdded { get; private set; } = [];

    public HashSet<PSTokenEntry> TokensModified { get; private set; } = [];

    public HashSet<PSTokenEntry> TokensRemoved { get; private set; } = [];

    public bool WrapValues => Values is object[] valueArray ? valueArray.Length > 3 : false;

    public object Values
    {
        get
        {
            if (Count > 0)
            {
                var tokens = _tokens.Where(token => Resources.ValueTokens.Contains(token.Kind));
                if (tokens.Any())
                {
                    var valuesArray = new string[tokens.Count()];
                    int count = 0;
                    foreach (var token in tokens)
                    {
                        valuesArray[count] = token.GetValue();
                    }

                    return valuesArray;
                }
            }

            return null;
        }
    }

    internal List<PSTokenEntry> Tokens { get; set; }

    internal bool TrackChanges { get; set; } = false;

    public PSTokenizedLine(int line, int indent)
    {
        Line = line;
        Indent = indent;
    }

    public void Add(PSTokenEntry token)
    {
        _tokens?.Add(token);
        OnInsertComplete(token);
    }

    public void Clear() => _tokens?.Clear();

    public bool Contains(PSTokenEntry token) => _tokens.Contains(token);

    public void CopyTo(PSTokenEntry[] tokens, int offset) => _tokens?.CopyTo(tokens, offset);

    public IEnumerator<PSTokenEntry> GetEnumerator() => _tokens?.GetEnumerator();

    public bool Remove(PSTokenEntry token)
    {
        var result = _tokens.Remove(token);
        OnRemoveComplete(token);
        return result;
    }

    internal virtual void OnInsertComplete(PSTokenEntry token)
    {
        if (TrackChanges)
        {
            TokensRemoved.Remove(token);
            TokensAdded.Add(token);
        }
    }

    internal virtual void OnRemoveComplete(PSTokenEntry token)
    {
        if (TrackChanges)
        {
            TokensRemoved.Add(token);
            TokensAdded.Remove(token);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}