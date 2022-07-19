using System.Collections.Generic;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public abstract class Inventory : IInventory
{
    public List<int> Items { get; }

    protected Inventory() => Items = new List<int>();

    protected Inventory(List<int> items) => Items = items;

    public abstract bool Add(int id);

    public abstract bool Remove(int id);
}