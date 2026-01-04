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
    
    // 추가: 생성된 맵 저장
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

    // 추가: 맵 생성 메서드
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
            LoadNode(_currentMap[0][0]).Forget();
        }
    }

    // 추가: 현재 맵 반환 메서드
    public List<List<MapNode>> GetCurrentMap()
    {
        return _currentMap;
    }

    public void MoveToNode(MapNode nextNode)
    {
        if (_isTransitioning) return;
        LoadNode(nextNode).Forget();
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
}