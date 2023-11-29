namespace PSDataFileHelper;

public interface IAstArray : IAstCollection
{
    bool ContainsNestedArrays { get; }

    bool IsLiteral { get; }

    bool ShouldWrapValues { get; }

    object GetValues();

    void SetValues(params object[] values);
}