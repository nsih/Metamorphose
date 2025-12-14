using UnityEngine;
using TMPro;
using Reflex.Attributes;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using System.Threading;
using Core;

public class RoomEventView : MonoBehaviour
{
    [SerializeField] private GameObject _panelRoot;
    [SerializeField] private TextMeshProUGUI _centerText;

    // RoomManager가 아니라, 그를 담고 있는 프로퍼티를 주입받습니다.
    private AsyncReactiveProperty<RoomManager> _currentRoomHandle;

    [Inject]
    public void Construct(AsyncReactiveProperty<RoomManager> currentRoomHandle)
    {
        _currentRoomHandle = currentRoomHandle;
    }

    void Start()
    {
        if (_currentRoomHandle == null) return;

        // [고급 기법] Switch 메서드 활용
        // _currentRoomHandle(방)이 바뀔 때마다 -> 그 방의 State(상태)를 새로 구독합니다.
        // 이전 방에 대한 구독은 자동으로 끊어줍니다. (메모리 누수 방지)
        
        _currentRoomHandle
            .Where(room => room != null) // null이 아닐 때만
            .SelectMany(room => room.CurrentRoomState) // 그 방의 State를 가져와서
            .Subscribe(state => HandleStateChange(state)) // 처리한다
            .AddTo(this.GetCancellationTokenOnDestroy());
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