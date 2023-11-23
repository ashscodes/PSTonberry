using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PSTonberry.Model;

internal class PSTokenizedLine : ICollection<IPSTokenEntry>
{
    private List<IPSTokenEntry> _tokens = [];

    public int Count => _tokens is not null ? _tokens.Count : 0;

    public string Identifier => _tokens?.FirstOrDefault(t => t.IsIdentifier)?.Text;

    public int Indent { get; }

    public bool IsClosingBrace => Count >= 2
                                  && _tokens[Count - 2].IsClosingBrace
                                  && _tokens[Count - 1].IsNewLine;

    public bool IsClosingParentheses => Count >= 2
                                        && _tokens[Count - 2].IsClosingParentheses
                                        && _tokens[Count - 1].IsNewLine;

    public bool IsComment => IsMultiLineComment || IsSingleLineComment;

    public bool IsEmptyLine => Count == 1 && _tokens[0].IsNewLine;

    public bool IsModified => _tokens is not null && _tokens.Any(t => t.IsModified());

    public bool IsMultiLineComment => Count > 2 && _tokens.All(t => t.IsComment || t.IsNewLine);

    public bool IsReadOnly => false;

    public bool IsSingleLineComment => Count == 2 && _tokens[0].IsComment && _tokens[1].IsNewLine;

    public int Line { get; }

    public bool ShouldWrapOnWrite => Values is object[] valueArray ? valueArray.Length > 3 : false;

    public HashSet<IPSTokenEntry> TokensAdded { get; private set; } = [];

    public HashSet<IPSTokenEntry> TokensModified { get; private set; } = [];

    public HashSet<IPSTokenEntry> TokensRemoved { get; private set; } = [];

    public object Values
    {
        get
        {
            var valueTokens = GetValueTokens()?.ToArray();
            if (valueTokens?.Length == 1)
            {
                return valueTokens[0].GetValue();
            }

            return valueTokens?.Select(token => token.GetValue()).ToArray();
        }
    }

    public IPSTokenEntry this[int index]
    {
        get => Count > 0 ? _tokens[index] : null;
    }

    internal PSTokenizedLine CommentContent { get; set; }

    internal List<IPSTokenEntry> Tokens
    {
        get => _tokens ?? [];
    }

    internal bool TrackChanges { get; set; } = false;

    public PSTokenizedLine(int line, int indent)
    {
        Line = line;
        Indent = indent;
    }

    public void Add(IPSTokenEntry token)
    {
        _tokens?.Add(token);
        OnInsertComplete(token);
    }

    public void Clear() => _tokens?.Clear();

    public bool Contains(IPSTokenEntry token) => _tokens is not null && _tokens.Contains(token);

    public void CopyTo(IPSTokenEntry[] tokens, int offset) => _tokens?.CopyTo(tokens, offset);

    public IEnumerator<IPSTokenEntry> GetEnumerator() => _tokens?.GetEnumerator();

    public bool IsIdentifierEqual(string identifier)
        => IsIdentifierEqualInLine(identifier) || IsIdentifierEqualInComment(identifier);

    public bool Remove(IPSTokenEntry token)
    {
        var result = _tokens.Remove(token);
        OnRemoveComplete(token);
        return result;
    }

    private IEnumerable<IPSTokenEntry> GetValueTokens()
        => _tokens?.Where(token => Resources.ValueTokens.Contains(token.Kind));

    private bool IsIdentifierEqualInComment(string identifier)
        => IsComment
           && !string.IsNullOrEmpty(CommentContent.Identifier)
           && CommentContent.Identifier.Equals(identifier, StringComparison.OrdinalIgnoreCase);

    private bool IsIdentifierEqualInLine(string identifier)
        => !string.IsNullOrEmpty(Identifier) && Identifier.Equals(identifier, StringComparison.OrdinalIgnoreCase);

    internal virtual void OnInsertComplete(IPSTokenEntry token)
    {
        if (TrackChanges)
        {
            TokensRemoved.Remove(token);
            TokensAdded.Add(token);
        }
    }

    internal virtual void OnRemoveComplete(IPSTokenEntry token)
    {
        if (TrackChanges)
        {
            TokensRemoved.Add(token);
            TokensAdded.Remove(token);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}