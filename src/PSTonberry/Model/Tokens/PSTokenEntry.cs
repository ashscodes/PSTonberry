using System.Management.Automation.Language;

namespace PSTonberry.Model;

internal abstract class PSTokenEntry
{
    public int Index { get; set; }

    public bool IsComment => Kind is TokenKind.Comment;

    public bool IsIdentifier => Kind is TokenKind.Identifier;

    public bool IsModified => CompareValue() != 0;

    public bool IsNewLine => Kind is TokenKind.NewLine;

    public PSTokenizedLine Parent { get; set; }

    public string Text { get; set; }

    public abstract TokenKind Kind { get; }

    internal bool IsReadOnly { get; set; } = false;

    internal bool TrackChanges { get; set; }

    public void SetParent(PSTokenizedLine parent) => Parent = parent;

    public abstract Token GetPSToken();

    public abstract string GetValue();

    public abstract override string ToString();

    internal abstract int CompareValue();
}