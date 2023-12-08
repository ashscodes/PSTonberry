using System.Collections.Generic;

namespace PSDataFileHelper;

public interface IPSDataFileCollection<T> : ICollection<T>, IPSDataFileObject
{
    string OriginalText { get; }

    T this[int index] { get; set; }

    void Move(int itemIndex, int targetIndex);
}