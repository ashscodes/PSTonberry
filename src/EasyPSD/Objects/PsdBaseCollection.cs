using System.Collections;
using System.Collections.Generic;

namespace EasyPSD;

public abstract class PsdBaseCollection : IPsdCollection<IPsdObject>, IPsdInlineComment
{
    private protected List<IPsdObject> _items = [];

    public CommentValue Comment { get; set; } = null;

    public int Count => _items is not null ? _items.Count : 0;

    public bool HasInlineComment => Comment is not null;

    public bool IsCollection => true;

    public bool IsReadOnly { get; set; } = false;

    public string OriginalText { get; set; }

    public IPsdObject this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    public PsdBaseCollection() { }

    public void Add(IPsdObject item) => _items.Add(item);

    public void Clear() => _items.Clear();

    public bool Contains(IPsdObject item) => Count > 0 && _items.Contains(item);

    public void CopyTo(IPsdObject[] array, int offset) => _items?.CopyTo(array, offset);

    public IEnumerator<IPsdObject> GetEnumerator() => _items.GetEnumerator();

    public bool Insert(int index, IPsdObject item)
    {
        if (Count > 0 && !Contains(item))
        {
            _items.Insert(index, item);
            return true;
        }

        return false;
    }

    public void Move(int currentIndex, int targetIndex) => _items.Move(currentIndex, targetIndex);

    public bool Remove(IPsdObject item) => Count > 0 && _items.Remove(item);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}