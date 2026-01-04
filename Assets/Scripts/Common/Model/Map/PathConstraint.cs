using System.Collections.Generic;
using Common;

[System.Serializable]
public class PathConstraint
{
    public int Layer;
    
    public bool HasRequiredType;
    public RoomType RequiredType;
    
    public List<RoomType> BannedTypes = new List<RoomType>();

    public PathConstraint()
    {
        HasRequiredType = false;
        BannedTypes = new List<RoomType>();
    }

    public bool IsTypeAllowed(RoomType type)
    {
        return !BannedTypes.Contains(type);
    }

    public PathConstraint(int layer)
    {
        Layer = layer;
        HasRequiredType = false;
        BannedTypes = new List<RoomType>();
    }
    
    public PathConstraint(int layer, RoomType requiredType)
    {
        Layer = layer;
        HasRequiredType = true;
        RequiredType = requiredType;
        BannedTypes = new List<RoomType>();
    }
}