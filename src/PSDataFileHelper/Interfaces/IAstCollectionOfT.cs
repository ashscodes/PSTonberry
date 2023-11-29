using System.Collections.Generic;

namespace PSDataFileHelper;

public interface IAstCollection : IAstObject, ICollection<IAstObject>
{
    IAstObject this[int index] { get; set; }
}