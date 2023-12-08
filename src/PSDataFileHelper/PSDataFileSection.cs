namespace PSDataFileHelper;

public class PSDataFileSection : PSDataFileMap
{
    public string Name { get; set; }

    public PSDataFileSection() : base() { }

    public PSDataFileSection(string name) : this() => Name = name;

    public PSDataFileSection(string name, string originalText) : this(name) => OriginalText = originalText;
}