using UnityEngine;
using TMPro;
using Reflex.Attributes;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using System.Threading;
using Common;
using System;

public class RoomEventView : MonoBehaviour
{
    [SerializeField] private GameObject _panelRoot;
    [SerializeField] private TextMeshProUGUI _centerText;

    // RoomManager가 아니라, 그를 담고 있는 프로퍼티를 주입받습니다.
    private AsyncReactiveProperty<RoomManager> _currentRoomHandle;

    private IDisposable _roomStateSubscription;

    [Inject]
    public void Construct(AsyncReactiveProperty<RoomManager> currentRoomHandle)
    {
        _currentRoomHandle = currentRoomHandle;
    }

    void Start()
    {
        if (_currentRoomHandle == null) return;

        _currentRoomHandle.Subscribe(room => 
        {
            // 이전 방의 상태 구독해제
            _roomStateSubscription?.Dispose(); 

            if (room != null)
            {
                // 새방 상태 구독, 그 연결 고리를 변수에 저장
                _roomStateSubscription = room.CurrentRoomState
                    .Subscribe(state => HandleStateChange(state));
                
                // 수동 dispose
            }
        }).AddTo(this.GetCancellationTokenOnDestroy()); // 방 핸들 구독은 게임 끝날 때까지 유지
    }

    private void OnDestroy()
    {
        // 혹시 구독 중인 상태에서 오브젝트가 파괴되면 정리
        _roomStateSubscription?.Dispose();
    }

    private void HandleStateChange(RoomState state)
    {
        // 기존과 동일
        switch (state)
        {
            case RoomState.Battle:
                _panelRoot.SetActive(true);
                _centerText.text = "BATTLE START";
                break;
            case RoomState.Complete:
                _centerText.text = "CLEAR!";
                break;
            default:
                _panelRoot.SetActive(false);
                break;
        }
    }
}