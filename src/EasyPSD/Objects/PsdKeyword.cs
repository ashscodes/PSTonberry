using System.Collections.Generic;

namespace EasyPSD;

public sealed class PsdKeyword : IPsdObject
{
    public List<PsdKeywordCondition> Condition { get; set; }

    public bool HasCondition => Condition is not null && Condition.Count > 0;

    public bool IsCollection => false;

    public bool IsReadOnly { get; set; } = false;

    public string Keyword { get; set; }

    public PsdKeyword() => Condition = [];

    public PsdKeyword(string keyword) : this() => Keyword = keyword;
}