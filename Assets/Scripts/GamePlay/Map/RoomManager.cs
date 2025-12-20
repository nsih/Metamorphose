using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using Core;                 // RoomState가 있는 네임스페이스
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Reflex.Attributes;    // DI

using Random = UnityEngine.Random;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private List<RoomWaveData> _possibleWaveDatas; // 랜덤으로 고를 시나리오들
    [SerializeField] private List<Transform> _spawnPoints; // 씬에 배치된 스폰 위치들


    // state
    private AsyncReactiveProperty<RoomState> _roomState = new AsyncReactiveProperty<RoomState>(RoomState.Idle);
    public IReadOnlyAsyncReactiveProperty<RoomState> CurrentRoomState => _roomState;

    private AsyncReactiveProperty<RoomManager> _globalCurrentRoomHandle; // 전역 등록용 핸들




    // logic
    private RoomWaveData _currentWaveData;
    private List<Enemy> _activeEnemies = new List<Enemy>();
    private EnemyFactory _factory; // 팩토리 패턴 주체
    private CancellationTokenSource _cts;


    // DI
    [Inject]
    public void Construct(AsyncReactiveProperty<RoomManager> globalHandle)
    {
        _globalCurrentRoomHandle = globalHandle;
    }

    private void Start()
    {
        // 팩토리 고용
        _factory = new EnemyFactory();

        // 전역
        if (_globalCurrentRoomHandle != null)
        {
            _globalCurrentRoomHandle.Value = this;
        }

        StartRoomEvent();
    }

    private void OnDestroy()
    {
        _roomState?.Dispose();
        
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetRoom();
        }
    }

    //Battle Flow 
    public void StartRoomEvent()
    {
        if (_roomState.Value != RoomState.Idle) return;

        //Debug.Log("Start Room event");
        _roomState.Value = RoomState.Battle;

        // 시나리오 랜덤 선택
        if (_possibleWaveDatas != null && _possibleWaveDatas.Count > 0)
        {
            int rnd = Random.Range(0, _possibleWaveDatas.Count);
            _currentWaveData = _possibleWaveDatas[rnd];
        }
        else
        {
            Debug.LogError("RoomWaveData error");
            return;
        }

        // 비동기 시나리오 시작
        _cts = new CancellationTokenSource();
        ProcessScenario(_cts.Token).Forget();
    }

    private async UniTaskVoid ProcessScenario(CancellationToken token)
    {
        try
        {
            for (int i = 0; i < _currentWaveData.Waves.Count; i++)
            {
                Wave wave = _currentWaveData.Waves[i];
                Debug.Log($"Wave {i + 1} Started");

                SpawnWaveUnits(wave);

                await UniTask.WaitUntil(() => 
                {
                    _activeEnemies.RemoveAll(x => x == null || !x.gameObject.activeSelf); 
                    
                    return _activeEnemies.Count == 0;
                }, cancellationToken: token);

                Debug.Log($"Wave {i + 1} Cleared");

                if (wave.PostWaveDelay > 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(wave.PostWaveDelay), cancellationToken: token);
                }
            }

            CompleteRoom();
        }
        catch (System.OperationCanceledException)
        {
            Debug.Log("웨이브 취소");
        }
        catch (Exception e)
        {
            Debug.LogError("Room Error");
        }
    }

    private void SpawnWaveUnits(Wave wave)
    {
        foreach (var entry in wave.SpawnGroups)
        {
            int index = 0;
            if (_spawnPoints.Count > 0)
                index = entry.SpawnPointIndex % _spawnPoints.Count;
            
            Transform spawnTr = _spawnPoints[index];



            for (int k = 0; k < entry.Count; k++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
                
                // 팩토리에게 주문 (Create 내부에서 Pool.Get -> Init(Data) -> SetReleaseAction 다 해줬잖아)
                Enemy enemy = _factory.Create(spawnTr.position + offset, entry.EnemyData);
                
                _activeEnemies.Add(enemy);
            }
        }
    }

    private void CompleteRoom()
    {
        Debug.Log("[Room] All Waves Cleared! Room Complete.");
        _roomState.Value = RoomState.Complete;
        
        // TODO: 문 열림 연출, 보상 상자 생성 등
    }

    public void ResetRoom()
    {
        // 웨이브 취소
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        // 수거
        foreach (var enemy in _activeEnemies)
        {
            if (enemy != null && enemy.gameObject.activeSelf)
            {
                EnemyPoolManager.Instance.Release(enemy);
            }
        }
        _activeEnemies.Clear();

        // 초기화하고 재시작
        _roomState.Value = RoomState.Idle;
        StartRoomEvent();
    }
}