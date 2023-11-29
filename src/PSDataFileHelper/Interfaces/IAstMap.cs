using System.Collections.Generic;

namespace PSDataFileHelper;

public interface IAstMap : IAstCollection
{
    List<DataSection> AdditionalSections { get; set; }

    ICollection<string> Keys { get; }

    IAstObject this[string keyName] { get; set; }

    bool ContainsSection(string sectionName);

    DataSection GetSection(string sectionName);
}

public interface IAstKeyValuePair : IAstObject, IAstObjectValue
{
    string Key { get; }
}