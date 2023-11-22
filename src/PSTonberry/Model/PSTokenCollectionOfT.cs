using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation.Language;

namespace PSTonberry.Model;

internal abstract class PSTokenCollection<T> : ICollection<PSTokenizedLine> where T : Ast
{
    private List<PSTokenizedLine> _values = [];

    public T Ast { get; set; }

    public int Count => _values.Count;

    public Exception Error { get; set; }

    public bool HasError => Error is not null;

    public bool IsReadOnly => false;

    public List<PSTokenizedLine> Lines { get; set; }

    public void Add(PSTokenizedLine item) => _values?.Add(item);

    public void Clear() => _values?.Clear();

    public bool Contains(PSTokenizedLine item) => _values is not null && _values.Contains(item);

    public void CopyTo(PSTokenizedLine[] array, int offset) => _values?.CopyTo(array, offset);

    public IEnumerator<PSTokenizedLine> GetEnumerator() => _values.GetEnumerator();

    public bool Remove(PSTokenizedLine item) => _values.Remove(item);

    public abstract void Write(StreamWriter writer);

    internal bool TryGetAstFromString(string sectionString, out T ast, out Token[] tokens)
    {
        ast = null;
        var scriptBlockAst = Parser.ParseInput(sectionString, out tokens, out ParseError[] errors);
        if (errors.Length > 0)
        {
            Error = new InvalidOperationException(string.Format(Resources.DataSectionCouldNotBeParsed, GetType().Name));
            return false;
        }

        ast = (T)scriptBlockAst.Find(i => i is T, false);
        return ast is not null;
    }

    internal static bool TryParseSection(string input, string identifier, out string section)
    {
        var startIndex = input.IndexOf(identifier);
        if (startIndex != -1)
        {
            var endOfCollection = input.CountNestedTokens('{', '}');
            if (endOfCollection > 0)
            {
                var endIndex = input.IndexOfN('}', endOfCollection) + 1;
                if (endIndex != -1)
                {
                    section = input[startIndex..endIndex];
                    return true;
                }
            }
        }

        section = string.Empty;
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}