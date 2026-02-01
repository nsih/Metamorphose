using System.Collections.Generic;
using UnityEngine;
using Common;
using System.Linq;

public class MapGenerator
{
    const int MAX_ATTEMPTS = 10;

    private MapGenerationConfig _config;
    private int _nodeIdCounter = 0;
    private List<(MapNode from, MapNode to)> _allConnections = new List<(MapNode, MapNode)>();

    public MapGenerator(MapGenerationConfig config)
    {
        _config = config;
    }

    public Map GenerateMap()
    {
        Map map;

        for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
        {
            _nodeIdCounter = 0;
            _allConnections.Clear();

            map = CreateMap();
            HashSet<MapNode> usedNodes = GeneratePaths(map);

            if (usedNodes.Count == 0)
            {
                continue;
            }

            RemoveOrphanNodes(map, usedNodes);
            map.StartNode.State = NodeState.Available;

            return map;
        }


        Debug.Log("Create fallback map...");
        return CreateFallbackMap();
    }

    Map CreateFallbackMap()
    {
        Map map = new Map();

        _nodeIdCounter = 0;
        _allConnections.Clear();

        MapNode startNode = CreateNode(0, 0, RoomType.Start);
        map.Nodes.Add(startNode);

        MapNode current = startNode;
        for (int layer = 1; layer < _config.LayerCount - 1; layer++)
        {
            MapNode node = CreateNode(layer, 0, RoomType.Battle);
            map.Nodes.Add(node);
            current.NextNodeIds.Add(node.NodeID);
            current = node;
        }

        MapNode bossNode = CreateNode(_config.LayerCount - 1, 0, RoomType.Boss);
        map.Nodes.Add(bossNode);
        current.NextNodeIds.Add(bossNode.NodeID);

        map.StartNode.State = NodeState.Available;
        return map;
    }

    private Map CreateMap()
    {
        Map map = new Map();

        for (int layer = 0; layer < _config.LayerCount; layer++)
        {
            List<MapNode> currentLayer = new List<MapNode>();

            if (layer == 0)
            {
                MapNode startNode = CreateNode(layer, 0, RoomType.Start);
                currentLayer.Add(startNode);
            }
            else if (layer == _config.LayerCount - 1)
            {
                MapNode bossNode = CreateNode(layer, 0, RoomType.Boss);
                currentLayer.Add(bossNode);
            }
            else
            {
                PathConstraint constraint = _config.GetConstraintForLayer(layer);

                if(constraint != null && constraint.HasRequiredType)
                {
                    for(int index = 0; index < _config.NodesPerLayer; index++)
                    {
                        MapNode node = CreateNode(layer, index, constraint.RequiredType);
                        currentLayer.Add(node);
                    }
                }
                else
                {
                    for(int index = 0; index < _config.NodesPerLayer; index++)
                    {
                        RoomType type = _config.RollRoomTypeWithConstraint(layer);
                        MapNode node = CreateNode(layer, index, type);
                        currentLayer.Add(node);
                    }
                }
            }

            map.Nodes.AddRange(currentLayer);
            map.LayerCount++;
        }

        return map;
    }

    private HashSet<MapNode> GeneratePaths(Map map)
    {
        HashSet<MapNode> usedNodes = new HashSet<MapNode>();
        int failedPaths = 0;
        int maxFailures = _config.PathCount * 2;

        for (int i = 0; i < _config.PathCount; i++)
        {
            List<MapNode> path = GenerateSinglePath(map);

            if(path == null || path.Count < map.LayerCount)
            {
                failedPaths++;
                i--;

                if(failedPaths >= maxFailures)
                {
                    return new HashSet<MapNode>();
                }
                continue;
            }

            foreach (var node in path)
            {
                usedNodes.Add(node);
            }

            ConnectPath(path);
        }

        return usedNodes;
    }

    private List<MapNode> GenerateSinglePath(Map map)
    {
        List<MapNode> path = new List<MapNode>();

        MapNode current = map.StartNode;
        path.Add(current);

        for (int layer = 1; layer < map.LayerCount; layer++)
        {
            List<MapNode> nextLayer = map.GetNodesInLayer(layer);
            List<MapNode> validNodes = GetNonCrossingAdjacentNodes(current, nextLayer);

            if(validNodes.Count == 0)
            {
                return null;
            }

            int randomIndex = Random.Range(0, validNodes.Count);
            current = validNodes[randomIndex];
            path.Add(current);
        }

        return path;
    }

    private List<MapNode> GetNonCrossingAdjacentNodes(MapNode from, List<MapNode> nextLayerNodes)
    {
        List<MapNode> validNodes = new List<MapNode>();

        foreach (var node in nextLayerNodes)
        {
            if (!IsAdjacent(from, node))
                continue;
            
            if (from.NextNodeIds.Contains(node.NodeID))
            {
                validNodes.Add(node);
                continue;
            }
            
            if (!WouldCrossExistingConnection(from, node))
            {
                validNodes.Add(node);
            }
        }

        return validNodes;
    }

    private bool WouldCrossExistingConnection(MapNode from, MapNode to)
    {
        foreach (var connection in _allConnections)
        {
            if (connection.from == from || connection.to == to)
                continue;
            
            if (connection.from.Layer != from.Layer)
                continue;

            if (DoLinesCross(from.IndexInLayer, to.IndexInLayer, 
                           connection.from.IndexInLayer, connection.to.IndexInLayer))
            {
                return true;
            }
        }

        return false;
    }

    private bool DoLinesCross(int fromA, int toA, int fromB, int toB)
    {
        bool aGoesDown = toA > fromA;
        bool bGoesDown = toB > fromB;
        
        if (aGoesDown == bGoesDown)
            return false;
        
        int minA = Mathf.Min(fromA, toA);
        int maxA = Mathf.Max(fromA, toA);
        int minB = Mathf.Min(fromB, toB);
        int maxB = Mathf.Max(fromB, toB);
        
        bool overlaps = minA < maxB && minB < maxA;
        
        return overlaps;
    }

    private void ConnectPath(List<MapNode> path)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            MapNode from = path[i];
            MapNode to = path[i + 1];

            if (!from.NextNodeIds.Contains(to.NodeID))
            {
                from.NextNodeIds.Add(to.NodeID);
                _allConnections.Add((from, to));
            }
        }
    }

    private MapNode CreateNode(int layer, int indexInLayer, RoomType type)
    {
        GameObject prefab = _config.GetPrefabForType(type);
        MapNode node = new MapNode(_nodeIdCounter++, layer, indexInLayer, type, prefab);
        return node;
    }

    public bool IsAdjacent(MapNode from, MapNode to)
    {
        if (to.Layer != from.Layer + 1)
        {
            return false;
        }

        if (from.Type == RoomType.Start)
        {
            return true;
        }

        if (to.Type == RoomType.Boss)
        {
            return true;
        }

        int indexDiff = Mathf.Abs(to.IndexInLayer - from.IndexInLayer);
        return indexDiff <= 1;
    }

    private void RemoveOrphanNodes(Map map, HashSet<MapNode> usedNodes)
    {
        for(int layer = 0; layer < map.LayerCount; layer++)
        {
            List<MapNode> currentLayer = map.GetNodesInLayer(layer);
            var orphanNodes = currentLayer.Where(node => !usedNodes.Contains(node));
            foreach (var node in orphanNodes)
            {
                map.Nodes.Remove(node);
            }
        }
    }

    #region Debug

    public void PrintGrid(List<List<MapNode>> grid)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== Grid ===");

        int totalNodes = 0;
        for (int i = 0; i < grid.Count; i++)
        {
            sb.Append($"Layer {i}: ");
            foreach (var node in grid[i])
            {
                sb.Append($"[{node.Type}] ");
            }
            sb.AppendLine($"({grid[i].Count})");
            totalNodes += grid[i].Count;
        }

        sb.AppendLine($"Total: {totalNodes} nodes");
        Debug.Log(sb.ToString());
    }

    public void PrintConnections(Map map)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== Connections ===");

        int totalConnections = 0;

        for(int layer = 0; layer < map.LayerCount; layer++)
        {
            var layerNodes = map.GetNodesInLayer(layer);
            foreach (var node in layerNodes)
            {
                if (node.NextNodeIds.Count > 0)
                {
                    sb.Append($"{node} -> ");
                    foreach (var nextId in node.NextNodeIds)
                    {
                        MapNode next = map.GetNode(nextId);
                        sb.Append($"{next} ");
                    }
                    sb.AppendLine();
                    totalConnections += node.NextNodeIds.Count;
                }
            }
        }

        sb.AppendLine($"Total: {totalConnections} connections");
        Debug.Log(sb.ToString());
    }

    public void PrintConnectionsDetailed(Map map)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== Connection Details ===");

        for (int layer = 0; layer < map.LayerCount - 1; layer++)
        {
            sb.AppendLine($"Layer {layer} -> {layer + 1}:");
            
            List<(int from, int to)> connections = new List<(int, int)>();
            
            var layerNodes = map.GetNodesInLayer(layer);
            foreach (var node in layerNodes)
            {
                foreach (var nextId in node.NextNodeIds)
                {
                    MapNode next = map.GetNode(nextId);
                    connections.Add((node.IndexInLayer, next.IndexInLayer));
                    sb.AppendLine($"  {node.IndexInLayer} -> {next.IndexInLayer}");
                }
            }
            
            for (int i = 0; i < connections.Count; i++)
            {
                for (int j = i + 1; j < connections.Count; j++)
                {
                    var a = connections[i];
                    var b = connections[j];
                    
                    bool aStartsAboveB = a.from < b.from;
                    bool aEndsAboveB = a.to < b.to;
                    
                    if (aStartsAboveB != aEndsAboveB)
                    {
                        sb.AppendLine($"  CROSS: ({a.from}->{a.to}) X ({b.from}->{b.to})");
                    }
                }
            }
        }

        Debug.Log(sb.ToString());
    }

    #endregion
}