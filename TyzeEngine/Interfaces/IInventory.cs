using System.Collections.Generic;

namespace TyzeEngine.Interfaces;

public interface IInventory
{
    List<int> Items { get; }

    bool Add(int id);
    bool Remove(int id);
}