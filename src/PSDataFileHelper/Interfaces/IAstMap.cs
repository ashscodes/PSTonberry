using System.Collections.Generic;

namespace PSDataFileHelper;

public interface IAstMap : IAstCollection
{
    List<DataSection> AdditionalSections { get; set; }

    ICollection<string> Keys { get; }

    IAstObject this[string keyName] { get; set; }
}

public interface IAstKeyValuePair : IAstObject, IAstObjectValue
{
    string Key { get; }
}