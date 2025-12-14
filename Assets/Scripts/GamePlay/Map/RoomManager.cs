using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using Core;                 // RoomState, RoomWaveData가 있는 네임스페이스
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq; // AsyncReactiveProperty 사용을 위해 필요
using Reflex.Attributes;    // [Inject] 사용을 위해 필요

// Random 모호함 해결을 위한 별칭 선언
using Random = UnityEngine.Random;

public class RoomManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private List<RoomWaveData> _possibleWaveDatas;

    [Header("Scene References")]
    [SerializeField] private List<Transform> _spawnPoints;

    // [핵심 1] 로컬 상태 관리: UI가 이 값을 구독하여 화면을 갱신함
    // 외부에서는 읽기만 가능(IReadOnly...)하도록 캡슐화
    private AsyncReactiveProperty<RoomState> _roomState = new AsyncReactiveProperty<RoomState>(RoomState.Idle);
    public IReadOnlyAsyncReactiveProperty<RoomState> CurrentRoomState => _roomState;

    // [핵심 2] 글로벌 등록용 핸들: Installer가 만든 전역 변수를 주입받음
    private AsyncReactiveProperty<RoomManager> _globalCurrentRoomHandle;

    // 내부 변수들
    private RoomWaveData _currentWaveData;
    private List<GameObject> _activeEnemies = new List<GameObject>();
    private CancellationTokenSource _cts;

    // 1. 의존성 주입 (Reflex)
    // 인스톨러가 생성해둔 '현재 방을 담는 그릇'을 받아옵니다.
    [Inject]
    public void Construct(AsyncReactiveProperty<RoomManager> globalHandle)
    {
        _globalCurrentRoomHandle = globalHandle;
    }

    private void Start()
    {
        // 2. 전역 등록
        // 게임이 시작되면(또는 이 방이 생성되면) "내가 현재 방이야!"라고 등록합니다.
        // 이를 구독 중인 UI가 "어, 방 바꼈네?" 하고 이 방의 _roomState를 새로 구독하게 됩니다.
        if (_globalCurrentRoomHandle != null)
        {
            _globalCurrentRoomHandle.Value = this;
        }

        StartRoomEvent();
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지: ReactiveProperty와 CTS는 반드시 해제해야 합니다.
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

    public void StartRoomEvent()
    {
        // 이미 배틀 중이거나 완료된 상태면 진입 차단 (.Value로 값 접근)
        if (_roomState.Value != RoomState.Idle) return;

        Debug.Log("Room Event Start");
        
        // 구독 중인 UI 자동 갱신
        _roomState.Value = RoomState.Battle;

        // 웨이브 데이터 랜덤 선택
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

        // 비동기 시나리오 시작
        _cts = new CancellationTokenSource();
        ProcessScenario(_cts.Token).Forget();
    }

    private async UniTaskVoid ProcessScenario(CancellationToken token)
    {
        try
        {
            // 웨이브 순차 실행
            for (int i = 0; i < _currentWaveData.Waves.Count; i++)
            {
                Wave wave = _currentWaveData.Waves[i];

                // 적 스폰
                SpawnWaveUnits(wave);

                // Polling: 적들이 모두 죽을 때까지 대기
                // (적이 Destroy되면 리스트에서 null이 되므로 RemoveAll로 정리)
                await UniTask.WaitUntil(() => 
                {
                    _activeEnemies.RemoveAll(x => x == null); 
                    return _activeEnemies.Count == 0;
                }, cancellationToken: token);

                Debug.Log($"{i + 1} Wave end");

                // 웨이브 간 잠시 대기 (연출 시간)
                await UniTask.Delay(1000, cancellationToken: token);
            }

            // 모든 웨이브 종료
            CompleteRoom();
        }
        catch (System.OperationCanceledException)
        {
            Debug.Log("웨이브 강제 취소됨 (리셋 등)");
        }
        catch (Exception e)
        {
            Debug.LogError($"Scenario Error: {e.Message}");
        }
    }

    private void SpawnWaveUnits(Wave wave)
    {
        foreach (var entry in wave.SpawnGroups)
        {
            int index = entry.SpawnPointIndex;
            
            // 스폰 포인트 인덱스 안전장치
            if (_spawnPoints.Count > 0)
            {
                index = index % _spawnPoints.Count; 
            }
            else 
            {
                Debug.LogError("Spawn point error");
                return;
            }

            Transform spawnTr = _spawnPoints[index];

            // Count만큼 적 생성
            for (int k = 0; k < entry.Count; k++)
            {
                //Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
                Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, 0);
                
                GameObject mob = Instantiate(entry.EnemyPrefab, spawnTr.position + offset, Quaternion.identity);
                
                _activeEnemies.Add(mob);
            }
        }
    }

    private void CompleteRoom()
    {
        Debug.Log("Room Clear");
        
        _roomState.Value = RoomState.Complete;
        
        // 보상 UI 구현해야함 호옹이
    }

    public void ResetRoom()
    {
        // 1. 비동기 작업 취소
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        // 살아있는 적 제거
        foreach (var enemy in _activeEnemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        _activeEnemies.Clear();

        // UI OFF
        _roomState.Value = RoomState.Idle;

        StartRoomEvent();
    }
}