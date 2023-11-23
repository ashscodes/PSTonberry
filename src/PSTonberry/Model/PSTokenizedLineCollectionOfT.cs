using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;

namespace PSTonberry.Model;

internal abstract class PSTokenizedLineCollection<T> : ICollection<PSTokenizedLine> where T : Ast
{
    private List<PSTokenizedLine> _lines = [];

    public T Ast { get; set; }

    public int Count => _lines.Count;

    public Exception Error { get; set; }

    public bool HasError => Error is not null;

    public bool IsReadOnly => false;

    public List<PSTokenizedLine> Lines { get; set; }

    public void Add(PSTokenizedLine item) => _lines?.Add(item);

    public void Clear() => _lines?.Clear();

    public bool Contains(PSTokenizedLine item) => _lines is not null && _lines.Contains(item);

    public void CopyTo(PSTokenizedLine[] array, int offset) => _lines?.CopyTo(array, offset);

    public IEnumerator<PSTokenizedLine> GetEnumerator() => _lines.GetEnumerator();

    public bool Remove(PSTokenizedLine item) => _lines.Remove(item);

    public bool TryGetLineByIdentifier(string identifier, out PSTokenizedLine lineWithIdentifier)
    {
        lineWithIdentifier = _lines.FirstOrDefault(l => l.IsIdentifierEqual(identifier));
        return lineWithIdentifier != null;
    }

    public bool TrySetIdentifierValue<V>(string identifier, V value)
    {
        if (TryGetLineByIdentifier(identifier, out PSTokenizedLine lineWithIdentifier))
        {

            return true;
        }

        return false;
    }

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

    internal static bool TryParseSection(
        string input,
        string identifier,
        out string newSection,
        out string modifiedInput)
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
                    newSection = input[startIndex..endIndex];
                    modifiedInput = input.Replace(newSection, null);
                    return true;
                }
            }
        }

        modifiedInput = input;
        newSection = string.Empty;
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}