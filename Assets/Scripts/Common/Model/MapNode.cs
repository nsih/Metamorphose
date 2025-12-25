using UnityEngine;
using System.Collections;
using Common;
using System.Collections.Generic;

[System.Serializable]
public class MapNode
{
    public int NodeID;          
    public RoomType Type;       
    public GameObject RoomPrefab;
    public List<MapNode> NextNodes = new List<MapNode>();

    public MapNode(int id, RoomType type, GameObject prefab)
    {
        NodeID = id;
        Type = type;
        RoomPrefab = prefab;
    }

    
}
