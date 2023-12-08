using System.Linq;

namespace PSDataFileHelper;

public sealed class PSDataFileArrayExpression : PSDataFileArray
{
    public PSDataFileArrayExpression() { }
}

public sealed class PSDataFileArrayLiteral : PSDataFileArray
{
    public PSDataFileArrayLiteral() { }
}

public abstract class PSDataFileArray : PSDataFileCollection
{
    bool ContainsNestedArrays => _items is not null && _items.Any(v => v is PSDataFileArray);

    public bool ShouldWrapValues => Count > 3;

    public PSDataFileArray() { }
}