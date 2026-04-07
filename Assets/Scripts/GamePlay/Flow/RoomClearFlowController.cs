// Assets/Scripts/GamePlay/Flow/RoomClearFlowController.cs
using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Common;
using Reflex.Attributes;
using R3;
using BulletPro;

public class RoomClearFlowController : MonoBehaviour
{
    // UI가 구독할 이벤트
    public event Action OnRequestRewardUI;
    public event Action OnRequestMapUI;

    // InteractPromptView가 참조
    public ClearPortal ActivePortal { get; private set; }

    private ReactiveProperty<RoomManager> _roomHandle;
    private IInputService _inputService;
    private TopDownCameraController _cameraController;
    private MapManager _mapManager;
    private GamePlay.RunEndManager _runEndManager;

    private readonly SerialDisposable _roomSub = new SerialDisposable();
    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    private CancellationTokenSource _flowCts;
    private bool _isFlowRunning;

    [Inject]
    public void Construct(
        ReactiveProperty<RoomManager> roomHandle,
        IInputService inputService,
        TopDownCameraController cameraController,
        MapManager mapManager,
        GamePlay.RunEndManager runEndManager)
    {
        _roomHandle = roomHandle;
        _inputService = inputService;
        _cameraController = cameraController;
        _mapManager = mapManager;
        _runEndManager = runEndManager;
        Debug.Log("RoomClearFlowController: Construct 호출");
    }

    private void Start()
    {
        Debug.Log($"RoomClearFlowController: Start, roomHandle null={_roomHandle == null}");

        _roomSub.AddTo(_disposables);

        _roomHandle
            .Subscribe(OnRoomChanged)
            .AddTo(_disposables);
    }

    private void OnDestroy()
    {
        _flowCts?.Cancel();
        _flowCts?.Dispose();
        _disposables.Dispose();
    }

    private void OnRoomChanged(RoomManager room)
    {
        Debug.Log($"RoomClearFlowController: OnRoomChanged, room null={room == null}");

        // 진행 중 흐름 취소
        _flowCts?.Cancel();
        _flowCts?.Dispose();
        _flowCts = null;
        _isFlowRunning = false;

        // 이전 포탈 정리
        CleanupPortal();

        if (room == null)
        {
            _roomSub.Disposable = Disposable.Empty;
            return;
        }

        // 전투방이 아니면 구독 안 함
        if (!IsCombatRoom())
        {
            Debug.Log("RoomClearFlowController: 전투방 아님, 구독 스킵");
            _roomSub.Disposable = Disposable.Empty;
            return;
        }

        Debug.Log("RoomClearFlowController: RoomState 구독 시작");

        // SerialDisposable로 이전 방 구독 자동 해제
        _roomSub.Disposable = room.CurrentRoomState
            .Subscribe(state =>
            {
                Debug.Log($"RoomClearFlowController: RoomState={state}, isFlowRunning={_isFlowRunning}");
                if (state == RoomState.Complete && !_isFlowRunning)
                {
                    _isFlowRunning = true;
                    _flowCts = new CancellationTokenSource();
                    RunClearFlowAsync(_flowCts.Token).Forget();
                }
            });
    }

    private async UniTaskVoid RunClearFlowAsync(CancellationToken ct)
    {
        try
        {
            RoomManager room = _roomHandle.Value;
            if (room == null) return;

            bool isBoss = IsBossRoom();
            Debug.Log($"RoomClearFlowController: 클리어 흐름 시작, isBoss={isBoss}");

            // 1) 입력 차단
            _inputService.SetEnabled(false);
            Debug.Log("RoomClearFlowController: 입력 차단");

            // 2) 적 탄막 전체 제거
            KillAllEnemyBullets();
            Debug.Log("RoomClearFlowController: 탄막 제거");

            // 3) 보스방이면 추가 딜레이
            if (isBoss)
            {
                await UniTask.Delay(1000, cancellationToken: ct);
            }

            // 4) 연출 텀
            await UniTask.Delay(500, cancellationToken: ct);

            // 5) 포탈 스폰
            Vector3 spawnPos = room.PortalSpawnPosition;
            Debug.Log($"RoomClearFlowController: 포탈 스폰 위치={spawnPos}, 프리팹 null={room.ClearPortalPrefab == null}");

            if (room.ClearPortalPrefab != null)
            {
                GameObject portalObj = Instantiate(room.ClearPortalPrefab, spawnPos, Quaternion.identity);
                ActivePortal = portalObj.GetComponent<ClearPortal>();
                Debug.Log($"RoomClearFlowController: 포탈 생성, ActivePortal null={ActivePortal == null}");
            }

            if (ActivePortal == null)
            {
                Debug.Log("RoomClearFlowController: 포탈 없음, 즉시 진행");
                HandlePortalEntered(isBoss);
                return;
            }

            // 6) 카메라 팬
            Debug.Log("RoomClearFlowController: 카메라 팬 시작");
            await _cameraController.PanToAsync(spawnPos, 0.6f, ct);
            Debug.Log("RoomClearFlowController: 카메라 팬 완료");

            // 7) 입력 복원
            _inputService.SetEnabled(true);
            Debug.Log("RoomClearFlowController: 입력 복원");

            // 8) 포탈 진입 대기
            var portalTcs = new UniTaskCompletionSource();

            ActivePortal.OnPortalEntered += () =>
            {
                portalTcs.TrySetResult();
            };

            await portalTcs.Task.AttachExternalCancellation(ct);

            // 9) 포탈 진입 후 입력 차단
            _inputService.SetEnabled(false);
            Debug.Log("RoomClearFlowController: 포탈 진입 처리");
            HandlePortalEntered(isBoss);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("RoomClearFlowController: 흐름 취소됨");
        }
    }

    private void HandlePortalEntered(bool isBoss)
    {
        if (isBoss)
        {
            _runEndManager.NotifyRunClear();
        }
        else
        {
            OnRequestRewardUI?.Invoke();
        }
    }

    public void NotifyRewardComplete()
    {
        OnRequestMapUI?.Invoke();
    }

    private void KillAllEnemyBullets()
    {
        BulletEmitter[] emitters = FindObjectsOfType<BulletEmitter>();
        foreach (var emitter in emitters)
        {
            if (emitter.GetComponentInParent<PlayerAttack>() != null)
                continue;

            emitter.Kill();
        }
    }

    private bool IsCombatRoom()
    {
        if (_mapManager == null || _mapManager.CurrentNode == null) return true;

        RoomType type = _mapManager.CurrentNode.Type;
        if (type == RoomType.Start || type == RoomType.Battle || type == RoomType.Elite || type == RoomType.Boss)
            return true;

        return false;
    }

    private bool IsBossRoom()
    {
        if (_mapManager == null || _mapManager.CurrentNode == null) return false;
        return _mapManager.CurrentNode.Type == RoomType.Boss;
    }

    private void CleanupPortal()
    {
        if (ActivePortal != null)
        {
            Destroy(ActivePortal.gameObject);
            ActivePortal = null;
        }
    }
}