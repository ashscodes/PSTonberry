using System.Collections;
using System.Collections.Generic;

namespace PSDataFileHelper;

public abstract class PSDataFileCollection : IPSDataFileCollection<IPSDataFileObject>
{
    private protected List<IPSDataFileObject> _items;

    public int Count => _items is not null ? _items.Count : 0;

    public bool IsCollection => true;

    public bool IsReadOnly { get; set; } = false;

    public string OriginalText { get; set; }

    public IPSDataFileObject this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    public PSDataFileCollection() { }

    public void Add(IPSDataFileObject item) => _items?.Add(item);

    public void Clear() => _items.Clear();

    public bool Contains(IPSDataFileObject item) => Count > 0 && _items.Contains(item);

    public void CopyTo(IPSDataFileObject[] array, int offset) => _items?.CopyTo(array, offset);

    public IEnumerator<IPSDataFileObject> GetEnumerator() => _items.GetEnumerator();

    public bool Insert(int index, IPSDataFileObject item)
    {
        if (Count > 0 && !Contains(item))
        {
            _items.Insert(index, item);
            return true;
        }

        return false;
    }

    public void Move(int currentIndex, int targetIndex) => _items.Move(currentIndex, targetIndex);

    public bool Remove(IPSDataFileObject item) => Count > 0 && _items.Remove(item);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}