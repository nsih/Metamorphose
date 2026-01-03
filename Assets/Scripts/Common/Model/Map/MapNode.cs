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


    //추상 좌표
    public int Layer;         
    public int IndexInLayer;   

    public NodeState State;



    public MapNode(int id, RoomType type, GameObject prefab)
    {
        NodeID = id;
        Type = type;
        RoomPrefab = prefab;
    }


    //상태 변경
    public void Unlock()
    {
        if (State == NodeState.Locked)
        {
            State = NodeState.Available;
        }
    }

    public void Complete()
    {
        State = NodeState.Completed;
    }

    public bool IsAccessible()
    {
        return State == NodeState.Available;
    }

    public override string ToString()
    {
        return $"[L{Layer}-{IndexInLayer}] {Type} (ID:{NodeID})";
    }

    
}
