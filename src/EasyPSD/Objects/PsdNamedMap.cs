namespace EasyPSD;

public class PsdNamedMap : PsdBaseMap
{
    public string Name { get; set; }

    public PsdNamedMap() : base() { }

    public PsdNamedMap(string name) : this() => Name = name;

    public PsdNamedMap(string name, string originalText) : this(name) => OriginalText = originalText;
}