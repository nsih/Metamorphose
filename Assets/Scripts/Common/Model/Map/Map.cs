using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Common
{
    public class Map
    {
        public List<MapNode> Nodes = new();
        public int LayerCount = 0;

        public MapNode StartNode => GetNode(0);

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
                node.CompleteAndUnlockNext();
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