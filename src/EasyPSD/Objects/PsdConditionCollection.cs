using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;

namespace EasyPSD;

public class PsdConditionCollection : IPsdCollection<IPsdCondition>, IPsdCondition
{
    private List<IPsdCondition> _items = [];

    private Token _token;

    public IPsdCondition this[int index]
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

    public bool IsArray => Count > 1 && _items.Any(i => i is ComparisonCondition || i is LogicalCondition);

    public bool IsCollection => true;

    public bool IsReadOnly => false;

    public PsdConditionCollection(Token token) => _token = token;

    public void Add(IPsdCondition item) => _items?.Add(item);

    public void Clear() => _items?.Clear();

    public bool Contains(IPsdCondition item) => Count > 0 && _items.Contains(item);

    public void CopyTo(IPsdCondition[] array, int offset) => _items?.CopyTo(array, offset);

    public IEnumerator<IPsdCondition> GetEnumerator() => _items?.GetEnumerator();

    public virtual string GetValue()
    {
        if (Count == 0)
        {
            return string.Empty;
        }

        string collectionOpen = _token is not null ? _token.Text : string.Empty;
        string collectionClose = _token is not null ? ")" : string.Empty;
        var builder = new StringBuilder(collectionOpen);
        for (int i = 0; i < Count; i++)
        {
            if (i != 0)
            {
                if (IsArray)
                {
                    builder.Append(',');
                }

                builder.Append(' ');
            }

            builder.Append(_items[i].GetValue());
        }

        builder.Append(collectionClose);
        return builder.ToString();
    }

    public void Move(int itemIndex, int targetIndex) => _items.Move(itemIndex, targetIndex);

    public bool Remove(IPsdCondition item) => Count > 0 && _items.Remove(item);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}