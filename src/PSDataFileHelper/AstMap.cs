using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PSDataFileHelper;

public abstract class AstMap : IAstMap
{
    protected private List<IAstObject> _mapItems = [];

    public List<DataSection> AdditionalSections { get; set; } = [];

    public int Count => _mapItems is not null ? _mapItems.Count : 0;

    public int Index { get; set; }

    public bool IsReadOnly { get; set; }

    public ICollection<string> Keys => (ICollection<string>)(_mapItems?.Where(i => i is IAstKeyValuePair kvp)
                                                                      ?.Select(kvp => ((IAstKeyValuePair)kvp).Key));

    public IAstObject this[string keyName]
    {
        get => GetMapKeyValuePair(keyName);
        set => SetMapKeyValuePairValue(keyName, value);
    }

    public IAstObject this[int index]
    {
        get => _mapItems[index] ?? null;
        set => SetMapKeyValuePairValue(_mapItems[index] ?? null, value);
    }

    public AstMap() { }

    public void Add(IAstObject item) => _mapItems?.Add(item);

    public void Clear() => _mapItems?.Clear();

    public bool Contains(IAstObject item) => _mapItems is not null && _mapItems.Contains(item);

    public bool ContainsSection(string sectionName)
    {
        if (AdditionalSections is null || AdditionalSections.Count == 0)
        {
            return false;
        }

        return AdditionalSections.Any(s => s.SectionName == sectionName);
    }

    public void CopyTo(IAstObject[] array, int offset) => _mapItems?.CopyTo(array, offset);

    public DataSection GetDataSection(string sectionName)
    {
        if (ContainsSection(sectionName))
        {
            return AdditionalSections.First(s => s.SectionName == sectionName);
        }

        return null;
    }

    public IEnumerator<IAstObject> GetEnumerator() => _mapItems?.GetEnumerator();

    public bool IsModified() => Count > 0 && _mapItems.Any(i => i.IsModified());

    public bool Remove(IAstObject item) => _mapItems is not null && _mapItems.Remove(item);

    public abstract override string ToString();

    private IAstKeyValuePair GetMapKeyValuePair(string keyName)
        => (IAstKeyValuePair)(_mapItems?.FirstOrDefault(i => i is IAstKeyValuePair kvp && kvp.Key == keyName));

    private object GetMapKeyValuePairValue(string keyName)
    {
        var mapItem = GetMapKeyValuePair(keyName);
        return mapItem?.Value;
    }

    private void SetMapKeyValuePairValue(string keyName, object value)
    {
        var mapItem = GetMapKeyValuePair(keyName);
        SetMapKeyValuePairValue(mapItem, value);
    }

    private void SetMapKeyValuePairValue(IAstObject mapItem, object value)
    {
        if (value is null)
        {
            return;
        }

        if (mapItem is not null && mapItem is IAstKeyValuePair kvp)
        {
            if (value is object[] values && !values.All(v => v is IAstKeyValuePair))
            {
                List<object> newValues = [];
                for (int i = 0; i < values.Length; i++)
                {
                    if (values[i] is IAstObjectValue astValue)
                    {
                        newValues.AddRange(astValue.Value);
                    }
                    else
                    {
                        newValues.Add(values[i]);
                    }
                }

                kvp.Value = newValues;
            }
            else if (value is IAstKeyValuePair newKvp)
            {
                kvp = newKvp;
            }
            else
            {
                kvp.Value = [value];
            }
        }
    }

    private bool TryGetMapKeyValuePairValue<T>(string keyName, out T value) where T : new()
    {
        value = default;
        var mapItem = GetMapKeyValuePair(keyName);
        if (mapItem?.Value is null || mapItem.Value.Count == 0)
        {
            return false;
        }

        if (mapItem.Value.Count == 1)
        {
            value = CastToObject<T>(mapItem.Value[0]);
            return true;
        }

        if (typeof(T) is IList list)
        {
            value = new T();
            for (int i = 0; i < mapItem.Value.Count; i++)
            {
                var newValue = CastToObject<T>(mapItem.Value[i]);
                ((IList)value).Add(newValue);
            }

            return true;
        }

        return false;
    }

    private static T CastToObject<T>(object value)
    {
        if (value is null)
        {
            return default(T);
        }

        if (value is T newValue)
        {
            return newValue;
        }

        return (T)Convert.ChangeType(value, typeof(T));
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}