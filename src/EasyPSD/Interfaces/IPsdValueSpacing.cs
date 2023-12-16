namespace EasyPSD;

public interface IPsdValueSpacing : IPsdObject
{
    bool HasPrecedingEmptyLine { get; set; }
}