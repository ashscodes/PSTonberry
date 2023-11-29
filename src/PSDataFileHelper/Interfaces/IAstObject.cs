using System.Collections.Generic;

namespace PSDataFileHelper;

public interface IAstObject
{
    int Index { get; set; }

    bool IsModified();
}

public interface IAstObjectValue
{
    List<object> Value { get; set; }
}