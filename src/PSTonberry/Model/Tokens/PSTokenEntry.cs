using System.Management.Automation.Language;

namespace PSTonberry.Model;

internal abstract class PSTokenEntry : IPSTokenEntry
{
    public IScriptExtent Extent { get; private set; }

    public bool HasError { get; private set; }

    public int Index { get; set; }

    public bool IsClosingBrace => Kind is TokenKind.RCurly;

    public bool IsClosingParentheses => Kind is TokenKind.RParen;

    public bool IsComment => Kind is TokenKind.Comment;

    public bool IsIdentifier => Kind is TokenKind.Identifier;

    public bool IsNewLine => Kind is TokenKind.NewLine;

    public TokenKind Kind { get; private set; }

    public string Text { get; set; }

    public TokenFlags? TokenFlags { get; private set; }

    internal bool IsReadOnly { get; set; } = false;

    internal bool TrackChanges { get; set; }

    public PSTokenEntry() { }

    internal PSTokenEntry(Token token, int index)
    {
        Extent = token.Extent;
        HasError = token.HasError;
        Kind = token.Kind;
        Text = token.Text;
        TokenFlags = token.TokenFlags;
    }

    public override string ToString() => Kind == TokenKind.EndOfInput ? "<eof>" : Text;

    public T Clone<T>() where T : PSTokenEntry, new()
    {
        var tokenEntry = new T();
        tokenEntry.Extent = Extent;
        tokenEntry.HasError = HasError;
        tokenEntry.Kind = Kind;
        tokenEntry.Text = Text;
        tokenEntry.TokenFlags = TokenFlags;
        return tokenEntry;
    }

    public abstract object GetValue();

    public abstract bool IsModified();
}

internal interface IPSTokenEntry
{
    IScriptExtent Extent { get; }

    bool HasError { get; }

    int Index { get; set; }

    bool IsClosingBrace { get; }

    bool IsClosingParentheses { get; }

    bool IsComment { get; }

    bool IsIdentifier { get; }

    bool IsNewLine { get; }

    TokenKind Kind { get; }

    string Text { get; set; }

    TokenFlags? TokenFlags { get; }

    T Clone<T>() where T : PSTokenEntry, new();

    object GetValue();

    bool IsModified();
}


internal interface IClonablePSToken<T> where T : PSTokenEntry
{
    T Clone();
}