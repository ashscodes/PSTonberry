using System.Linq;

namespace EasyPSD;

public sealed class PsdArrayExpression : PsdArray
{
    public bool HasAtSymbol { get; set; }

    public PsdArrayExpression() { }
}

public sealed class PsdArrayLiteral : PsdArray
{
    public PsdArrayLiteral() { }
}

public abstract class PsdArray : PsdBaseCollection
{
    bool ContainsNestedArrays => _items is not null && _items.Any(v => v is PsdArray);

    public bool ShouldWrapValues => Count > 3;

    public PsdArray() { }
}