using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using Common;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Reflex.Attributes;
using System.Linq;

using Random = UnityEngine.Random;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private List<RoomWaveData> _possibleWaveDatas;
    [SerializeField] private List<Transform> _spawnPoints;

    private AsyncReactiveProperty<RoomState> _roomState = new AsyncReactiveProperty<RoomState>(RoomState.Idle);
    public IReadOnlyAsyncReactiveProperty<RoomState> CurrentRoomState => _roomState;

    private AsyncReactiveProperty<RoomManager> _globalCurrentRoomHandle;
    private EnemyPoolManager _enemyPoolManager;
    private MapManager _mapManager;

    private RoomWaveData _currentWaveData;
    private List<Enemy> _activeEnemies = new List<Enemy>();
    private EnemyFactory _factory;
    
    private PlayerSpawner _playerSpawner;
    private Transform _playerTransform;
    
    private CancellationTokenSource _cts;

    [Inject]
    public void Construct(
        AsyncReactiveProperty<RoomManager> globalHandle, 
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
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetRoom();
        }
    }

    public void StartRoomEvent()
    {
        if (_roomState.Value != RoomState.Idle) return;

        _roomState.Value = RoomState.Battle;

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
                Enemy enemy = _factory.Create(spawnTr.position + offset, entry.EnemyData);
                _activeEnemies.Add(enemy);
            }
        }
    }

    private void CompleteRoom()
    {
        _roomState.Value = RoomState.Complete;
        
        if (_mapManager != null && _mapManager.CurrentNode != null)
        {
            _mapManager.CurrentNode.CompleteAndUnlockNext();
        }
    }

    private void CleanupRoom()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        foreach (var enemy in _activeEnemies)
        {
            if (enemy != null && enemy.gameObject.activeSelf)
            {
                _enemyPoolManager.Release(enemy);
            }
        }
        _activeEnemies.Clear();
    }

    public void ResetRoom()
    {
        CleanupRoom();
        _roomState.Value = RoomState.Idle;
        UniTask.DelayFrame(1).ContinueWith(StartRoomEvent).Forget();
    }
}