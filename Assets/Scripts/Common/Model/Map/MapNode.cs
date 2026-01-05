using UnityEngine;
using System.Collections.Generic;
using Common;

/// <summary>
/// 맵의 개별 노드
/// 추상 좌표(Layer, IndexInLayer)로 위치 표현
/// </summary>
[System.Serializable]
public class MapNode
{
    public int NodeID;          
    public RoomType Type;       
    public GameObject RoomPrefab;
    public List<MapNode> NextNodes = new List<MapNode>();

    // 추상 좌표
    public int Layer;           
    public int IndexInLayer;    
    
    // 상태
    public NodeState State;

    public MapNode(int id, int layer, int indexInLayer, RoomType type, GameObject prefab)
    {
        NodeID = id;
        Layer = layer;
        IndexInLayer = indexInLayer;
        Type = type;
        RoomPrefab = prefab;
        State = NodeState.Locked;
    }

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

    public void CompleteAndUnlockNext()
    {
        Complete();
        foreach (var next in NextNodes)
        {
            next.Unlock();
        }
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