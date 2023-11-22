using System.Collections;
using System.Management.Automation.Language;

namespace PSTonberry.Model;
internal abstract class PSHashtableData : PSTokenCollection<HashtableAst>
{
    internal void Init(string astString)
    {
        if (TryGetAstFromString(astString, out HashtableAst hashtableAst, out Token[] tokens))
        {
            Ast = hashtableAst;
            Lines = tokens.ToTokenizedLines();
        }
    }

    internal bool TryGetSafeValue(out Hashtable item)
    {
        try
        {
            item = (Hashtable)Ast.SafeGetValue();
            return true;
        }
        catch
        {
            item = default;
            return false;
        }
    }
}