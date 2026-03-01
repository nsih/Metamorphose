using System.Collections.Generic;

public class AcquiredItemRegistry
{
    private readonly HashSet<string> _acquiredUniqueIds = new();

    public void Register(string itemId)
    {
        _acquiredUniqueIds.Add(itemId);
    }

    public bool IsAcquired(string itemId)
    {
        return _acquiredUniqueIds.Contains(itemId);
    }

    public void Clear()
    {
        _acquiredUniqueIds.Clear();
    }
}