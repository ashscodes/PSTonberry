using System.Collections.Generic;

namespace EasyPSD;

public interface IPsdCollection<T> : ICollection<T>, IPsdObject
{
    T this[int index] { get; set; }

    void Move(int itemIndex, int targetIndex);
}