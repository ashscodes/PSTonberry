using System.Collections;
using System.Collections.Generic;

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

    public override string ToString() => Keyword;
}

public sealed class PsdKeywordCollection : IPsdCollection<PsdKeyword>
{
    private List<PsdKeyword> _items = [];

    public PsdKeyword this[int index]
    {
        get
        {
            Ensure.IndexIsInRange(index, Count);
            return _items[index];
        }
        set
        {
            Ensure.IndexIsInRange(index, Count);
            _items[index] = value;
        }
    }

    public int Count => _items is not null ? _items.Count : 0;

    public bool IsCollection => true;

    public bool IsReadOnly => false;

    public void Add(PsdKeyword item) => _items?.Add(item);

    public void Clear() => _items?.Clear();

    public bool Contains(PsdKeyword item) => Count > 0 && _items.Contains(item);

    public void CopyTo(PsdKeyword[] array, int offset) => _items?.CopyTo(array, offset);

    public IEnumerator<PsdKeyword> GetEnumerator() => _items?.GetEnumerator();

    public void Move(int itemIndex, int targetIndex) => _items.Move(itemIndex, targetIndex);

    public bool Remove(PsdKeyword item) => Count > 0 && _items.Remove(item);

    public override string ToString()
        => Count > 0 ? string.Join("; ", _items) : typeof(PsdKeywordCollection).FullName;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}