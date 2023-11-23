using System.Collections;
using System.Linq;
using System.Text;

namespace PSTonberry.Model;

internal class PSHashtable
{
    private Hashtable _hashtable;

    private StringBuilder _builder;

    public int Count => _hashtable is not null ? _hashtable.Count : 0;

    public object this[int index] => _hashtable is not null ? _hashtable[index] : null;

    public PSHashtable(Hashtable hashtable) => _hashtable = hashtable;

    public override string ToString()
    {
        _builder = new StringBuilder();
        if (_hashtable is not null)
        {
            int indent = 0;
            WriteHashtable(_hashtable, ref indent);
        }

        return _builder.ToString();
    }

    public static explicit operator PSHashtable(Hashtable hashtable) => new(hashtable);

    public static implicit operator Hashtable(PSHashtable psHashtable) => psHashtable._hashtable;

    private static string GetIndent(int indent) => new string(' ', indent);

    private void WriteArray(object[] array, ref int indent)
    {
        _builder.Append("@(");
        if (array is null || array.Length == 0)
        {
            _builder.AppendLine(")");
            return;
        }

        var hasNestedArrays = array.Any(i => i is object[]);
        var hasNestedHashtables = array.Any(i => i is Hashtable);
        string separator = ", ";
        if (hasNestedArrays || hasNestedHashtables)
        {
            indent += 4;
            separator = ",\n";
        }

        for (int i = 0; i < array.Length; i++)
        {
            if (i != 0)
            {
                _builder.Append(separator);
            }

            if (array[i] is Hashtable hashtable)
            {
                WriteHashtable(hashtable, ref indent, true);
            }
            else if (array[i] is object[] nestedArray)
            {
                WriteArray(nestedArray, ref indent);
            }
            else
            {
                if (hasNestedArrays || hasNestedHashtables)
                {
                    if (i == 0)
                    {
                        _builder.AppendLine();
                    }

                    WriteValue(array[i], indent);
                }
                else
                {
                    WriteValue(array[i]);
                }
            }
        }

        if (hasNestedArrays || hasNestedHashtables)
        {
            indent -= 4;
            _builder.AppendLine();
            WriteIndented(")", indent);
        }
        else
        {
            _builder.Append(')');
        }
    }

    private void WriteHashtable(Hashtable hashtable, ref int indent, bool isInArray = false)
    {
        if (isInArray)
        {
            WriteIndentedLine("@{", indent);
        }
        else
        {
            _builder.AppendLine("@{");
        }

        indent += 4;
        foreach (DictionaryEntry entry in hashtable)
        {
            WriteDictionaryEntry(entry, ref indent);
        }

        indent -= 4;
        WriteIndented("}", indent);
    }

    private void WriteDictionaryEntry(DictionaryEntry entry, ref int indent)
    {
        indent += 4;
        WriteIndented($"{entry.Key} = ", indent);
        if (entry.Value is object[] array)
        {
            WriteArray(array, ref indent);
        }
        else if (entry.Value is Hashtable hashtable)
        {
            WriteHashtable(hashtable, ref indent);
        }
        else
        {
            WriteValue(entry.Value);
        }

        _builder.AppendLine();
        indent -= 4;
    }

    private void WriteIndented(string text, int indent)
    {
        _builder.Append(GetIndent(indent) + text);
    }

    private void WriteIndentedLine(string line, int indent)
    {
        _builder.AppendLine(GetIndent(indent) + line);
    }

    private void WriteValue(object value, int? indent = null)
    {
        string strValue;
        if (value is string tempString)
        {
            strValue = tempString;
            if (!tempString.StartsWith('"'))
            {
                strValue = '"' + strValue;
            }

            if (!tempString.EndsWith('"'))
            {
                strValue += '"';
            }
        }
        else if (value is bool booleanValue)
        {
            strValue = booleanValue ? "$true" : "$false";
        }
        else
        {
            strValue = value.ToString();
        }

        if (indent.HasValue)
        {
            WriteIndented(strValue, indent.Value);
            return;
        }

        _builder.Append(strValue);
    }
}