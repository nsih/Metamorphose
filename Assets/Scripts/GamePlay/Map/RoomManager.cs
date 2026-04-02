using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using Common;
using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using System.Linq;
using R3;
using UnityEngine.InputSystem;

using Random = UnityEngine.Random;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private List<RoomWaveData> _possibleWaveDatas;
    [SerializeField] private List<Transform> _spawnPoints;
    [SerializeField] private StageBossPoolSO _bossPool;

    private ReactiveProperty<RoomState> _roomState = new ReactiveProperty<RoomState>(RoomState.Idle);
    public ReadOnlyReactiveProperty<RoomState> CurrentRoomState => _roomState;

    private ReactiveProperty<RoomManager> _globalCurrentRoomHandle;
    private EnemyPoolManager _enemyPoolManager;
    private MapManager _mapManager;

    private RoomWaveData _currentWaveData;
    private List<Enemy> _activeEnemies = new List<Enemy>();
    private EnemyFactory _factory;

    private PlayerSpawner _playerSpawner;
    private Transform _playerTransform;

    private CancellationTokenSource _cts;

    // 보스방 전용
    private BossController _activeBoss;

    [Inject]
    public void Construct(
        ReactiveProperty<RoomManager> globalHandle,
        PlayerSpawner spawner,
        EnemyPoolManager enemyPoolManager,
        MapManager mapManager)
    {
        _globalCurrentRoomHandle = globalHandle;
        _playerSpawner = spawner;
        _enemyPoolManager = enemyPoolManager;
        _mapManager = mapManager;
    }

    private void Start()
    {
        if (_playerSpawner != null)
        {
            GameObject playerObj = _playerSpawner.Spawn();
            if (playerObj != null)
                _playerTransform = playerObj.transform;
        }

        _factory = new EnemyFactory(_playerTransform, _enemyPoolManager);

        if (_globalCurrentRoomHandle != null)
        {
            _globalCurrentRoomHandle.Value = this;
        }

        StartRoomEvent();
    }

    private void OnDestroy()
    {
        CleanupRoom();
        _roomState?.Dispose();
    }

    private void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetRoom();
        }
    }

    public void StartRoomEvent()
    {
        if (_roomState.Value != RoomState.Idle) return;

        // 보스방 분기
        if (_mapManager != null && _mapManager.CurrentNode != null)
        {
            if (_mapManager.CurrentNode.Type == RoomType.Boss)
            {
                _roomState.Value = RoomState.Battle;
                SpawnBoss();
                return;
            }
        }

        // 일반 웨이브 흐름
        if (_possibleWaveDatas != null && _possibleWaveDatas.Count > 0)
        {
            int rnd = Random.Range(0, _possibleWaveDatas.Count);
            _currentWaveData = _possibleWaveDatas[rnd];
        }
        else
        {
            Debug.LogError("RoomManager: wave data null");
            return;
        }

        _roomState.Value = RoomState.Battle;
        _cts = new CancellationTokenSource();
        ProcessScenario(_cts.Token).Forget();
    }

    private void SpawnBoss()
    {
        if (_bossPool == null)
        {
            Debug.LogError("RoomManager: bossPool null");
            CompleteRoom();
            return;
        }

        GameObject prefab = _bossPool.GetRandomBoss();

        if (prefab == null)
        {
            Debug.LogError("RoomManager: 보스 프리팹 없음, 방 즉시 완료");
            CompleteRoom();
            return;
        }

        Vector3 spawnPos = Vector3.zero;
        if (_spawnPoints != null && _spawnPoints.Count > 0)
        {
            spawnPos = _spawnPoints[0].position;
        }

        GameObject bossObj = Instantiate(prefab, spawnPos, Quaternion.identity);
        _activeBoss = bossObj.GetComponent<BossController>();

        if (_activeBoss == null)
        {
            Debug.LogError("RoomManager: BossController 없음");
            Destroy(bossObj);
            CompleteRoom();
            return;
        }

        // BossHealthBarView는 ReactiveProperty<RoomManager> 구독으로 자체 바인딩
        // RoomManager에서 직접 참조하지 않음 (어셈블리 순환참조 방지)

        _activeBoss.OnBossDeath += OnBossDefeated;
        Debug.Log("RoomManager: 보스 스폰");
    }

    private void OnBossDefeated()
    {
        Debug.Log("RoomManager: 보스 처치, 방 클리어");
        CompleteRoom();
    }

    private async UniTaskVoid ProcessScenario(CancellationToken token)
    {
        try
        {
            for (int i = 0; i < _currentWaveData.Waves.Count; i++)
            {
                Wave wave = _currentWaveData.Waves[i];

                SpawnWaveUnits(wave);

                await UniTask.WaitUntil(() =>
                {
                    return _activeEnemies.All(x => x == null || !x.gameObject.activeSelf);
                }, cancellationToken: token);

                _activeEnemies.Clear();

                if (wave.PostWaveDelay > 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(wave.PostWaveDelay), cancellationToken: token);
                }
            }

            CompleteRoom();
        }
        catch (OperationCanceledException) { }
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
                Enemy enemy = _factory.Create(spawnTr.position + offset, entry.EnemyBrain);
                _activeEnemies.Add(enemy);
            }
        }
    }

    private void CompleteRoom()
    {
        _roomState.Value = RoomState.Complete;

        if (_mapManager != null && _mapManager.CurrentNode != null)
        {
            var currentNode = _mapManager.CurrentNode;
            _mapManager.CurrentMap.CompleteAndUnlockNextNode(currentNode.NodeID);
        }
    }

    private void CleanupRoom()
    {
        // 웨이브 CancellationToken 정리
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        // 일반 적 풀 반환
        foreach (var enemy in _activeEnemies)
        {
            if (enemy != null && enemy.gameObject.activeSelf)
            {
                _enemyPoolManager.Release(enemy);
            }
        }
        _activeEnemies.Clear();

        // 보스 정리
        if (_activeBoss != null)
        {
            _activeBoss.OnBossDeath -= OnBossDefeated;

            if (_activeBoss.gameObject != null)
            {
                Destroy(_activeBoss.gameObject);
            }

            _activeBoss = null;
        }
    }

    public void ResetRoom()
    {
        CleanupRoom();
        _roomState.Value = RoomState.Idle;
        UniTask.DelayFrame(1).ContinueWith(StartRoomEvent).Forget();
    }
}