namespace PSDataFileHelper;

public interface IAstValue : IAstObject, IAstObjectValue
{
    bool IsReadOnly { get; }

    void SetValue(object value);
}