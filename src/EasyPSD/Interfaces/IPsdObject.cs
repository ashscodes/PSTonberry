namespace EasyPSD;

public interface IPsdObject
{
    bool IsCollection { get; }

    bool IsReadOnly { get; }
}