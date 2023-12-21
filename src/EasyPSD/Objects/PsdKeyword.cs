namespace EasyPSD;

public sealed class PsdKeyword : IPsdObject
{
    public PsdConditionCollection Condition { get; set; }

    public bool HasCondition => Condition is not null && Condition.Count > 0;

    public bool IsCollection => false;

    public bool IsReadOnly { get; set; } = false;

    public string Keyword { get; set; }

    public PsdScriptblock Scriptblock { get; set; }

    public PsdKeyword(string keyword) => Keyword = keyword;
}