using UnityEngine;
using Cysharp.Threading.Tasks;
using Common;
using Reflex.Core;
using Reflex.Attributes;
using Reflex.Injectors;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [SerializeField] private Transform _roomParent;
    [SerializeField] private MapGenerationConfig _config;

    [Inject] private Container _container;

    public MapNode CurrentNode { get; private set; }
    
    private GameObject _currentRoomInstance;
    private bool _isTransitioning = false;
    
    private List<List<MapNode>> _currentMap;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        GenerateMap();
    }

    private void GenerateMap()
    {
        if (_config == null)
        {
            Debug.LogError("MapManager: MapGenerationConfig null");
            return;
        }

        MapGenerator generator = new MapGenerator(_config);
        _currentMap = generator.GenerateMap();

        generator.PrintGrid(_currentMap);
        generator.PrintConnections(_currentMap);

        if (_currentMap.Count > 0 && _currentMap[0].Count > 0)
        {
            MapNode startNode = _currentMap[0][0];
            startNode.Unlock();
            LoadNode(startNode).Forget();
        }
    }

    public List<List<MapNode>> GetCurrentMap()
    {
        return _currentMap;
    }

    public void MoveToNode(MapNode nextNode)
    {
        if (_isTransitioning)
        {
            Debug.LogWarning("이미 방 전환 중입니다");
            return;
        }
        
        if (nextNode.State != NodeState.Available)
        {
            Debug.LogWarning($"접근 불가능한 노드: {nextNode} (State: {nextNode.State})");
            return;
        }
        
        LockOtherNodesInLayer(nextNode);
        
        LoadNode(nextNode).Forget();
    }

    private void LockOtherNodesInLayer(MapNode selectedNode)
    {
        if (_currentMap == null) return;

        int targetLayer = selectedNode.Layer;
        
        if (targetLayer < 0 || targetLayer >= _currentMap.Count) return;

        var layer = _currentMap[targetLayer];
        
        foreach (var node in layer)
        {
            if (node != selectedNode && node.State == NodeState.Available)
            {
                node.State = NodeState.Locked;
                Debug.Log($"노드 잠금: {node}");
            }
        }
    }

    private async UniTaskVoid LoadNode(MapNode node)
    {
        _isTransitioning = true;

        try
        {
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

                Debug.Log($"방 생성: {node.Type} (ID: {node.NodeID})");
            }
            else
            {
                Debug.LogError($"Node {node.NodeID} prefab null");
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
            GameObject player = newSpawner.Spawn();
            Debug.Log($"플레이어 위치 초기화: {player.transform.position}");
        }
        else
        {
            var player = GameObject.Find("Player");
            if (player != null)
            {
                player.transform.position = Vector3.zero;
                Debug.Log("플레이어 원점으로 이동");
            }
            else
            {
                Debug.LogWarning("플레이어를 찾을 수 없습니다");
            }
        }
    }
}