namespace PSDataFileHelper;

public abstract class DataSection : AstMap
{
    public string SectionName { get; set; }

    public DataSection() { }

    public DataSection(string sectionName) => SectionName = sectionName;
}