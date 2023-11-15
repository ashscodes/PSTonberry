using System.Collections.Generic;
using System.Management.Automation.Language;

namespace PSTonberry.Model;

internal class PSDataFileAst<T> where T : Ast
{
    public T Ast { get; set; }

    public ScriptBlockAst ScriptBlock { get; set; }

    public List<PSDataFileTokenCollection> Tokens { get; set; }

    internal PSDataFileAst(ScriptBlockAst scriptBlock, Token[] tokens)
    {
        ScriptBlock = scriptBlock;
        Tokens = tokens.ToPSDataTokenCollection();
    }

    internal bool TryFindAst(bool includeNested = false)
    {
        var Ast = (T)ScriptBlock.Find(i => i is T, includeNested);
        if (Ast is not null)
        {
            return true;
        }

        return false;
    }

    internal bool TryGetSafeValue<O>(out O value)
    {
        try
        {
            value = (O)Ast.SafeGetValue();
            return true;
        }
        catch
        {
            value = default;
            return false;
        }
    }

    internal bool TryParseSection(string sectionName, out string section)
    {
        if (ScriptBlock is not null)
        {
            var scriptBlockAst = ScriptBlock.ToString();
            var startIndex = scriptBlockAst.IndexOf(sectionName);
            if (startIndex != -1)
            {
                var collectionCount = scriptBlockAst.CountCollectionTokens('{', '}');
                if (collectionCount > 0)
                {
                    var endIndex = scriptBlockAst.IndexOfN('}', collectionCount) + 1;
                    if (endIndex != -1)
                    {
                        section = scriptBlockAst[startIndex..endIndex];
                        return true;
                    }
                }
            }
        }

        section = string.Empty;
        return false;
    }
}