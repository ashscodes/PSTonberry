namespace EasyPSD;

public interface IPsdValue<T> : IPsdObject
{
    bool HasValue { get; }

    T GetValue();

    void SetValue(T value);

    bool TrySetValue(object value);
}