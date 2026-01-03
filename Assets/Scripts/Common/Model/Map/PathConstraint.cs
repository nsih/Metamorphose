using System.Collections.Generic;
using Common;


[System.Serializable]
public class PathConstraint
{
    public int Layer;
    public RoomType? RequiredType;
    public List<RoomType> BannedTypes = new List<RoomType>();

    public PathConstraint()
    {
        BannedTypes = new List<RoomType>();
    }

    public bool IsTypeAllowed(RoomType type)
    {
        return !BannedTypes.Contains(type);
    }
}