namespace EasyPSD;

public sealed class PsdKeywordCondition : IPsdInlineComment, IPsdObject
{
    public CommentValue Comment { get; set; }

    public bool HasInlineComment => Comment is not null;

    public bool IsCollection => false;

    public bool IsReadOnly { get; set; } = false;

    public string Operator { get; set; }

    public object Reference { get; set; }

    public PsdScriptblock Scriptblock { get; set; }

    public object Value { get; set; }

    public PsdKeywordCondition(object reference, string comparisonOperator, object value)
    {
        Operator = comparisonOperator;
        Reference = reference;
        Value = value;
    }
}