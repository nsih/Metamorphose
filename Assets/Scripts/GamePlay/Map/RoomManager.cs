using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using Core;

public class RoomManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private List<RoomWaveData> _possibleWaveDatas;

    [Header("Scene References")]
    [SerializeField] private List<Transform> _spawnPoints;

    private RoomState _currentState = RoomState.Idle;
    private RoomWaveData _currentWaveData;
    
    


    private List<GameObject> _activeEnemies = new List<GameObject>();
    
    private CancellationTokenSource _cts;

    private void Start()
    {
        StartRoomEvent();   //나중에 프리팹으로 불러오면 온로드로 변경
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
        if (_currentState != RoomState.Idle) return;

        Debug.Log("Room Event Start");
        _currentState = RoomState.Battle;


        if (_possibleWaveDatas != null && _possibleWaveDatas.Count > 0)
        {
            int rnd = Random.Range(0, _possibleWaveDatas.Count);
            _currentWaveData = _possibleWaveDatas[rnd];
            Debug.Log($"Selected Data: {_currentWaveData.name}");
        }
        else
        {
            Debug.LogError("WaveData error");
            return;
        }

        _cts = new CancellationTokenSource();
        ProcessScenario(_cts.Token).Forget();
    }



    private async UniTaskVoid ProcessScenario(CancellationToken token)
    {
        try
        {
            // n개의 웨이브만큼 실행
            for (int i = 0; i < _currentWaveData.Waves.Count; i++)
            {
                Wave wave = _currentWaveData.Waves[i];

                //sqawn
                SpawnWaveUnits(wave);

                // polling
                await UniTask.WaitUntil(() => 
                {
                    _activeEnemies.RemoveAll(x => x == null); 
                    return _activeEnemies.Count == 0;
                }, cancellationToken: token);

                Debug.Log($"{i + 1} Wave end");


                await UniTask.Delay(1000, cancellationToken: token);
            }

            CompleteRoom();
        }
        catch (System.OperationCanceledException)
        {
            Debug.Log("웨이브 강제 재시작");
        }
    }

    private void SpawnWaveUnits(Wave wave)
    {
        foreach (var entry in wave.SpawnGroups)
        {
            int index = entry.SpawnPointIndex;
            if (_spawnPoints.Count > 0)
            {
                index = index % _spawnPoints.Count; 
            }
            else 
            {
                Debug.LogError("spawn point error");
                return;
            }

            Transform spawnTr = _spawnPoints[index];

            // Count만큼 스폰
            for (int k = 0; k < entry.Count; k++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
                
                GameObject mob = Instantiate(entry.EnemyPrefab, spawnTr.position + offset, Quaternion.identity);
                
                _activeEnemies.Add(mob);
            }
        }
    }

    // end
    private void CompleteRoom()
    {
        Debug.Log("Room Clear");
        _currentState = RoomState.Complete;
        // 보상 다음 맵 UI
    }

    public void ResetRoom()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        foreach (var enemy in _activeEnemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        _activeEnemies.Clear();

        _currentState = RoomState.Idle;
        StartRoomEvent();
    }
}