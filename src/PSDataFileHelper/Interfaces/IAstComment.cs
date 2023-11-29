namespace PSDataFileHelper;

public interface IAstComment : IAstObject, IAstObjectValue
{
    bool IsInMap { get; }

    bool IsMultiline { get; }

    bool TryGetKeyValuePair(out IAstKeyValuePair astKeyValuePair);
}
