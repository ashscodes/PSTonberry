using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;

namespace PSTonberry.Model;

// Implement CollectionBase for modified values
internal class PSDataToken : CollectionBase
{
    public string Identifier => IsIdentifier ? Token.Text : null;

    public int Index { get; }

    public bool IsComment => Token is not null && Token.Kind is TokenKind.Comment;

    public bool IsModified => Value is not null && Value.Length > 0;

    public bool IsIdentifier => Token is not null && Token.Kind is TokenKind.Identifier;

    public bool IsNewLine => Token is not null && Token.Kind is TokenKind.NewLine;

    public string Text { get; }

    public Token Token { get; }

    public string[] Value { get; private set; } = [];

    public PSDataToken(Token token, int index)
    {
        Token = token;
        Text = token.Text;
        Index = index;
    }

    public void SetValue(params string[] values) => Value = values;
}

internal class PSDataTokenCollection : ICollection<PSDataToken>
{
    private List<PSDataToken> _tokens = [];

    public int Count => _tokens.Count;

    public string Identifier => _tokens?.FirstOrDefault(token => token.IsIdentifier)?.Identifier;

    public int Indent { get; }

    public bool IsEmptyLine => Count == 1 && _tokens[0].IsNewLine;

    public bool IsReadOnly => false;

    public int Line { get; }

    public PSDataTokenCollection(int line, int indent)
    {
        Line = line;
        Indent = indent;
    }

    public void Add(Token token, int index) => Add(new PSDataToken(token, index));

    public void Add(PSDataToken token) => _tokens?.Add(token);

    public void Clear() => _tokens?.Clear();

    public bool Contains(PSDataToken token) => _tokens.Contains(token);

    public void CopyTo(PSDataToken[] tokens, int offset) => _tokens?.CopyTo(tokens, offset);

    public IEnumerator<PSDataToken> GetEnumerator() => _tokens?.GetEnumerator();

    public bool Remove(PSDataToken token) => _tokens.Remove(token);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
