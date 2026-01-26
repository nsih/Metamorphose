using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Common
{
    [Serializable]
    public class Map
    {
        public List<MapNode> Nodes = new();
        public int LayerCount = 0;

        public MapNode StartNode => Nodes[0] ?? null;
        public MapNode BossNode => Nodes[Nodes.Count - 1] ?? null;

        public MapNode GetNode(int id)
        {
            foreach (var node in Nodes)
            {
                if (node.NodeID == id)
                {
                    return node;
                }
            }

            Debug.LogError($"Map: Node with id {id} not found");
            return null;
        }

        public List<MapNode> GetNodesInLayer(int layer)
        {
            return Nodes.Where(node => node.Layer == layer).ToList();
        }

        public bool IsNodeConnected(int fromId, int toId)
        {
            var fromNode = GetNode(fromId);
            if (fromNode == null)
            {
                Debug.LogError($"Map: Node with id {fromId} not found");
                return false;
            }

            return fromNode.NextNodeIds.Contains(toId);
        }

        public void UnlockNode(int id)
        {
            var node = GetNode(id);
            if (node != null)
            {
                node.Unlock();
            }
        }

        public void CompleteNode(int id)
        {
            var node = GetNode(id);
            if (node != null)
            {
                node.Complete();
            }
        }

        public void CompleteAndUnlockNextNode(int id)
        {
            var node = GetNode(id);
            if (node != null)
            {
                node.Complete();
                foreach (var nextNodeId in node.NextNodeIds)
                {
                    UnlockNode(nextNodeId);
                }
            }
        }

        public bool IsNodeAccessible(int id)
        {
            var node = GetNode(id);
            if (node != null)
            {
                return node.IsAccessible();
            }
            return false;
        }
    }
}