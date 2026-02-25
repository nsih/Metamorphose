using UnityEngine;
using TMPro;
using Reflex.Attributes;
using Common;
using System;
using R3;

public class RoomEventView : MonoBehaviour
{
    [SerializeField] private GameObject _panelRoot;
    [SerializeField] private TextMeshProUGUI _centerText;

    private ReactiveProperty<RoomManager> _currentRoomHandle;

    private IDisposable _roomHandleSubscription;
    private IDisposable _roomStateSubscription;

    [Inject]
    public void Construct(ReactiveProperty<RoomManager> currentRoomHandle)
    {
        _currentRoomHandle = currentRoomHandle;
    }

    private void Start()
    {
        if (_currentRoomHandle == null) return;

        _roomHandleSubscription = _currentRoomHandle
            .Subscribe(room =>
            {
                _roomStateSubscription?.Dispose();

                if (room == null) return;

                _roomStateSubscription = room.CurrentRoomState
                    .Subscribe(state => HandleStateChange(state));
            });
    }

    private void OnDestroy()
    {
        _roomHandleSubscription?.Dispose();
        _roomStateSubscription?.Dispose();
    }

    private void HandleStateChange(RoomState state)
    {
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