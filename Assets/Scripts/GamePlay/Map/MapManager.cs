using UnityEngine;
using Cysharp.Threading.Tasks;
using Common;
using Reflex.Core;
using Reflex.Attributes;
using Reflex.Injectors;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    [SerializeField] private Transform _roomParent;
    [SerializeField] private MapGenerationConfig _config;

    [Inject] private Container _container;
    [Inject] private PlayerSpawner _playerSpawner;

    public MapNode CurrentNode { get; private set; }
    public Map CurrentMap => _currentMap;

    private GameObject _currentRoomInstance;
    private bool _isTransitioning = false;
    [SerializeField] private Map _currentMap;

    private void Start()
    {
        GenerateMap();
    }

    private void GenerateMap()
    {
        if (_config == null)
        {
            Debug.LogError("MapManager: config null");
            return;
        }

        MapGenerator generator = new MapGenerator(_config);
        _currentMap = generator.GenerateMap();

        if (_currentMap.StartNode != null)
        {
            _currentMap.StartNode.Unlock();
            LoadNode(_currentMap.StartNode).Forget();
        }
    }

    public void MoveToNode(MapNode nextNode)
    {
        if (_isTransitioning)
        {
            Debug.Log("already transitioning");
            return;
        }

        if (nextNode.State != NodeState.Available)
        {
            Debug.Log($"node not available: {nextNode}");
            return;
        }

        LockOtherNodesInLayer(nextNode);
        LoadNode(nextNode).Forget();
    }

    private void LockOtherNodesInLayer(MapNode selectedNode)
    {
        if (_currentMap == null) return;

        int targetLayer = selectedNode.Layer;
        if (targetLayer < 0 || targetLayer >= _currentMap.LayerCount) return;

        var layerNodes = _currentMap.GetNodesInLayer(targetLayer);

        foreach (var node in layerNodes)
        {
            if (node != selectedNode && node.State == NodeState.Available)
                node.State = NodeState.Locked;
        }
    }

    private async UniTaskVoid LoadNode(MapNode node)
    {
        _isTransitioning = true;

        try
        {
            // Room 전환 전 총알 정리
            GameObject playerObj = _playerSpawner?.GetPlayer();
            if (playerObj != null)
            {
                var playerAttack = playerObj.GetComponent<PlayerAttack>();
                playerAttack?.StopAndReset();
            }

            if (_currentRoomInstance != null)
            {
                Destroy(_currentRoomInstance);
                await UniTask.Yield();
            }

            CurrentNode = node;

            if (node.RoomPrefab != null)
            {
                _currentRoomInstance = Instantiate(node.RoomPrefab, _roomParent);
                _currentRoomInstance.name = $"Room_{node.NodeID}_{node.Type}";

                GameObjectInjector.InjectRecursive(_currentRoomInstance, _container);
                ResetPlayerPosition();
            }
            else
            {
                Debug.LogError($"node {node.NodeID} prefab null");
            }
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    private void ResetPlayerPosition()
    {
        var newSpawner = _currentRoomInstance?.GetComponentInChildren<PlayerSpawner>();

        if (newSpawner != null)
        {
            newSpawner.Spawn();
        }
        else
        {
            var player = GameObject.Find("Player");
            if (player != null)
                player.transform.position = Vector3.zero;
        }
    }
}