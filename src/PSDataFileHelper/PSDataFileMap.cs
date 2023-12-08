using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSDataFileHelper;

public abstract class PSDataFileMap : PSDataFileCollection
{
    public List<PSDataFileSection> AdditionalSections { get; set; } = [];

    public PSDataFileMap() { }

    public bool ContainsKey(string key)
        => _items.Any(i => i is PSDataFileMapEntry mapEntry && mapEntry.Key == key);

    public bool ContainsSection(string name)
        => AdditionalSections.Any(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public virtual PSDataFileSection GetSection(string name, bool recurse = false)
    {
        if (ContainsSection(name))
        {
            return AdditionalSections.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        if (recurse)
        {
            foreach (var section in AdditionalSections)
            {
                if (section.ContainsSection(name))
                {
                    return section.GetSection(name, recurse);
                }
            }
        }

        return null;
    }

    public PSDataFileMapEntry GetMapEntry(string key)
        => (PSDataFileMapEntry)_items.FirstOrDefault(i => i is PSDataFileMapEntry mapEntry && mapEntry.Key == key);

    public void InsertSection(int index, PSDataFileSection section) => AdditionalSections.Insert(index, section);

    public bool Remove(string key)
    {
        if (ContainsKey(key))
        {
            var item = GetMapEntry(key);
            return _items.Remove(item);
        }

        return false;
    }

    public virtual void AddArray(string name, object[] values, bool isInCoreObject = true)
    {
        Ensure.StringNotNullOrEmpty(name, nameof(name));
        Ensure.ArrayNotEmpty(values, nameof(values));
        var arrayValues = GetArray(values);
        AddMapEntry(name, arrayValues, isInCoreObject);
    }

    public virtual void AddComment(string value, bool isInCoreObject = true)
    {
        Ensure.StringNotNullOrEmpty(value, nameof(value));
        var comment = new PSDataFileComment(value);
        AddComment(comment, isInCoreObject);
    }

    public virtual void AddComment(string[] values, bool isInCoreObject = true)
    {
        Ensure.ArrayNotEmpty(values, nameof(values));
        var sb = new StringBuilder();
        sb.AppendLine("<#");
        for (int i = 0; i < values.Length; i++)
        {
            sb.AppendLine(values[i]);
        }

        sb.AppendLine("#>");
        var comment = new PSDataFileComment(sb.ToString());
        AddComment(comment, isInCoreObject);
    }

    public virtual void AddDoubleQuotedStringValue(string name, string value, bool isInCoreObject = true)
    {
        Ensure.StringNotNullOrEmpty(name, nameof(name));
        Ensure.StringNotNullOrEmpty(value, nameof(value));
        var stringExpandable = new PSDataFileStringExpandableValue(RemoveQuotationCharacter(value, '"'));
        AddMapEntry(name, stringExpandable, isInCoreObject);
    }

    public virtual void AddHashtable(string name, Hashtable hashtable, bool isInCoreObject = true)
    {
        Ensure.StringNotNullOrEmpty(name, nameof(name));
        Ensure.CollectionNotEmpty(hashtable, nameof(hashtable));
        var mapValues = GetMap(name, hashtable);
        AddMapEntry(name, mapValues, isInCoreObject);
    }

    public virtual void AddSingleQuotedStringValue(string name, string value, bool isInCoreObject = true)
    {
        Ensure.StringNotNullOrEmpty(name, nameof(name));
        Ensure.StringNotNullOrEmpty(value, nameof(value));
        var stringLiteral = new PSDataFileStringLiteralValue(RemoveQuotationCharacter(value, '\''));
        AddMapEntry(name, stringLiteral, isInCoreObject);
    }

    public virtual void AddVariableValue(string name,
                                         string variableName,
                                         bool isSplattedVariable = false,
                                         bool isInCoreObject = true)
    {
        Ensure.StringNotNullOrEmpty(name, nameof(name));
        Ensure.StringNotNullOrEmpty(variableName, nameof(variableName));
        var variableValue = new PSDataFileVariableValue(RemoveVariableCharacter(variableName))
        {
            IsSplattedVariable = isSplattedVariable
        };

        AddMapEntry(name, variableValue, isInCoreObject);
    }

    internal virtual void AddComment(PSDataFileComment value, bool isInCoreObject = true)
    {
        if (!isInCoreObject && this is PSDataFile dataFile)
        {
            dataFile.AdditionalData.Add(value);
            return;
        }

        Add(value);
    }

    internal virtual void AddMapEntry(string name, IPSDataFileObject value, bool isInCoreObject = true)
    {
        var mapEntry = new PSDataFileMapEntry(name, value);
        if (!isInCoreObject && this is PSDataFile dataFile)
        {
            dataFile.AdditionalData.Add(mapEntry);
            return;
        }

        Add(mapEntry);
    }

    internal PSDataFileArray GetArray(object[] values)
    {
        var arrayValue = new PSDataFileArrayExpression();
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] is null)
            {
                continue;
            }

            IPSDataFileObject value = GetCollectionValue(values[i]);
            arrayValue.Add(value);
        }

        return arrayValue;
    }

    internal IPSDataFileObject GetCollectionValue(object value)
    {
        return value switch
        {
            bool => GetBooleanValue(value),
            decimal or double or float or int or long => GetNumberValue(value),
            string strValue => new PSDataFileStringLiteralValue(strValue),
            Hashtable hashtable => GetMap(hashtable),
            object[] objectArray => GetArray(objectArray),
            _ => new PSDataFileStringLiteralValue((string)value)
        };
    }

    // Only used when map is in an array.
    internal PSDataFileMap GetMap(Hashtable hashtable)
    {
        var nestedMap = new PSDataFileNestedMap();
        return SetMapValues(nestedMap, hashtable);
    }

    internal PSDataFileMap GetMap(string name, Hashtable hashtable)
    {
        var dataSection = new PSDataFileSection(name);
        return SetMapValues(dataSection, hashtable);
    }

    internal PSDataFileMap SetMapValues(PSDataFileMap map, Hashtable hashtable)
    {
        foreach (DictionaryEntry entry in hashtable)
        {
            if (entry.Value is null)
            {
                continue;
            }

            var value = GetCollectionValue(entry.Value);
            map.Add(new PSDataFileMapEntry((string)entry.Key, value));
        }

        return map;
    }

    internal static PSDataFileBooleanValue GetBooleanValue(object value)
    {
        var booleanValue = new PSDataFileBooleanValue();
        booleanValue.TrySetValue(value);
        return booleanValue;
    }

    internal static PSDataFileNumberValue GetNumberValue(object value)
    {
        var numberValue = new PSDataFileNumberValue();
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