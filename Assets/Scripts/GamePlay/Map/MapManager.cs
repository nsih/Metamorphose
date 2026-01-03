using UnityEngine;
using UnityEngine.UI; 
using Cysharp.Threading.Tasks; 
using Common;
using Reflex.Core;
using Reflex.Attributes;
using Reflex.Injectors;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [SerializeField] private Transform _roomParent; // 방이 생성될 부모 (Hierarchy 정리용)

    [Header("Dummy Data")]
    [SerializeField] private GameObject _battleRoomPrefab;


    [Inject] private Container _container;

    

    public MapNode CurrentNode { get; private set; }
    
    private GameObject _currentRoomInstance; 
    
    // [Safety] 중복 실행 방지용 플래그
    private bool _isTransitioning = false; 

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
        GenerateDummyMap(); 
    }

    // 임시 맵 생성
    private void GenerateDummyMap()
    {
        // 노드 생성
        /*
        MapNode startNode = new MapNode(0, RoomType.Battle, _battleRoomPrefab);
        MapNode nodeA = new MapNode(1, RoomType.Battle, _battleRoomPrefab);
        MapNode nodeB = new MapNode(2, RoomType.Shop, _battleRoomPrefab); 

        // [Start] -> [A], [B] -> [Start] //임시
        startNode.NextNodes.Add(nodeA);
        startNode.NextNodes.Add(nodeB);

        nodeA.NextNodes.Add(startNode);
        nodeB.NextNodes.Add(startNode);

        // 첫 방 로드
        LoadNode(startNode).Forget();
        */
    }

    // 맵 UI가 호출할 코드
    public void MoveToNode(MapNode nextNode)
    {
        if (_isTransitioning) return;
        LoadNode(nextNode).Forget();
    }

    // 맵 참조해서 방 교체
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

            // Player.Instance.SetPosition(spawnPoint); 
        }
        finally
        {
            _isTransitioning = false;
        }
    }
}