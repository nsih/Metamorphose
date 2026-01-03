using UnityEngine;
using Common;

[System.Serializable]
public class RoomPrefabMapping
{
    public RoomType Type;
    public GameObject Prefab;

    public RoomPrefabMapping()
    {
        Type = RoomType.Battle;
    }

    public RoomPrefabMapping(RoomType type, GameObject prefab)
    {
        Type = type;
        Prefab = prefab;
    }
}