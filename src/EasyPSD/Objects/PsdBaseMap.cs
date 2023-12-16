using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyPSD;

public abstract class PsdBaseMap : PsdBaseCollection
{
    public List<PsdNamedMap> NamedMaps { get; set; } = [];

    public PsdBaseMap() { }

    public bool ContainsKey(string key)
        => _items.Any(i => i is PsdMapEntry mapEntry && mapEntry.Key == key);

    public bool ContainsNamedMap(string name)
        => NamedMaps.Any(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public virtual PsdNamedMap GetNamedMap(string name, bool recurse = false)
    {
        if (ContainsNamedMap(name))
        {
            return NamedMaps.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        if (recurse)
        {
            foreach (var namedMap in NamedMaps)
            {
                if (namedMap.ContainsNamedMap(name))
                {
                    return namedMap.GetNamedMap(name, recurse);
                }
            }
        }

        return null;
    }

    public PsdMapEntry GetMapEntry(string key)
        => (PsdMapEntry)_items.FirstOrDefault(i => i is PsdMapEntry mapEntry && mapEntry.Key == key);

    public void InsertNamedMap(int index, PsdNamedMap namedMap) => NamedMaps.Insert(index, namedMap);

    public bool Remove(string key)
    {
        if (ContainsKey(key))
        {
            var item = GetMapEntry(key);
            return _items.Remove(item);
        }

        return false;
    }

    public virtual void AddArray(string name, object[] values)
    {
        Ensure.StringNotNullOrEmpty(name, nameof(name));
        Ensure.ArrayNotEmpty(values, nameof(values));
        var arrayValues = GetArray(values);
        AddMapEntry(name, arrayValues);
    }

    public virtual void AddComment(string value)
    {
        Ensure.StringNotNullOrEmpty(value, nameof(value));
        var comment = new CommentValue(value);
        Add(comment);
    }

    public virtual void AddComment(string[] values)
    {
        Ensure.ArrayNotEmpty(values, nameof(values));
        var sb = new StringBuilder();
        sb.AppendLine("<#");
        for (int i = 0; i < values.Length; i++)
        {
            sb.AppendLine(values[i]);
        }

        sb.AppendLine("#>");
        var comment = new CommentValue(sb.ToString());
        Add(comment);
    }

    public virtual void AddDoubleQuotedString(string name, string value)
    {
        Ensure.StringNotNullOrEmpty(name, nameof(name));
        Ensure.StringNotNullOrEmpty(value, nameof(value));
        var stringExpandable = new DoubleQuotedString(RemoveQuotationCharacter(value, '"'));
        AddMapEntry(name, stringExpandable);
    }

    public virtual void AddHashtable(string name, Hashtable hashtable)
    {
        Ensure.StringNotNullOrEmpty(name, nameof(name));
        Ensure.CollectionNotEmpty(hashtable, nameof(hashtable));
        var mapValues = GetMap(name, hashtable);
        AddMapEntry(name, mapValues);
    }

    public virtual void AddSingleQuotedString(string name, string value)
    {
        Ensure.StringNotNullOrEmpty(name, nameof(name));
        Ensure.StringNotNullOrEmpty(value, nameof(value));
        var stringLiteral = new SingleQuotedString(RemoveQuotationCharacter(value, '\''));
        AddMapEntry(name, stringLiteral);
    }

    public virtual void AddVariable(string name,
                                    string variableName,
                                    bool isSplattedVariable = false)
    {
        Ensure.StringNotNullOrEmpty(name, nameof(name));
        Ensure.StringNotNullOrEmpty(variableName, nameof(variableName));
        var variableValue = new VariableValue(RemoveVariableCharacter(variableName))
        {
            IsSplattedVariable = isSplattedVariable
        };

        AddMapEntry(name, variableValue);
    }

    internal virtual void AddMapEntry(string name, IPsdObject value)
    {
        var mapEntry = new PsdMapEntry(name, value);
        if (value is PsdNamedMap namedMap)
        {
            namedMap.Name = name;
            NamedMaps.Add(namedMap);
        }

        Add(mapEntry);
    }

    internal PsdArray GetArray(object[] values)
    {
        var arrayValue = new PsdArrayExpression();
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] is null)
            {
                continue;
            }

            IPsdObject value = GetCollectionValue(values[i]);
            arrayValue.Add(value);
        }

        return arrayValue;
    }

    internal IPsdObject GetCollectionValue(object value)
    {
        return value switch
        {
            bool => GetBooleanValue(value),
            decimal or double or float or int or long => GetNumberValue(value),
            string strValue => new SingleQuotedString(strValue),
            Hashtable hashtable => GetMap(hashtable),
            object[] objectArray => GetArray(objectArray),
            _ => new SingleQuotedString((string)value)
        };
    }

    // Only used when map is in an array.
    internal PsdBaseMap GetMap(Hashtable hashtable)
    {
        var nestedMap = new PsdNestedMap();
        return SetMapValues(nestedMap, hashtable);
    }

    internal PsdBaseMap GetMap(string name, Hashtable hashtable)
    {
        var namedMap = new PsdNamedMap(name);
        return SetMapValues(namedMap, hashtable);
    }

    internal PsdBaseMap SetMapValues(PsdBaseMap map, Hashtable hashtable)
    {
        foreach (DictionaryEntry entry in hashtable)
        {
            if (entry.Value is null)
            {
                continue;
            }

            var value = GetCollectionValue(entry.Value);
            map.Add(new PsdMapEntry((string)entry.Key, value));
        }

        return map;
    }

    internal static BooleanValue GetBooleanValue(object value)
    {
        var booleanValue = new BooleanValue();
        booleanValue.TrySetValue(value);
        return booleanValue;
    }

    internal static NumberValue GetNumberValue(object value)
    {
        var numberValue = new NumberValue();
        numberValue.TrySetValue(value);
        return numberValue;
    }

    internal static string RemoveQuotationCharacter(string value, char symbol)
    {
        if (value.StartsWith(symbol))
        {
            value = value.TrimStart(symbol);
        }

        if (value.EndsWith(symbol))
        {
            value = value.TrimEnd(symbol);
        }

        return value;
    }

    internal static string RemoveVariableCharacter(string value)
    {
        if (value.StartsWith('@'))
        {
            value = value.TrimStart('@');
        }

        if (value.StartsWith('$'))
        {
            value = value.TrimStart('$');
        }

        return value;
    }
}