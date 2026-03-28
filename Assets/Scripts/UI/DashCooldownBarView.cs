using UnityEngine;
using UnityEngine.UI;
using Reflex.Attributes;
using R3;
using System;

public class DashCooldownBarView : MonoBehaviour
{
    [Inject] private PlayerModel _model;

    [SerializeField] private Slider _slider;

    private CompositeDisposable _disposables = new CompositeDisposable();

    void Start()
    {
        if (_model == null)
        {
            Debug.LogError("DashCooldownBarView: DI 오류");
            return;
        }

        _model.DashCooldownNormalized
            .Subscribe(val =>
            {
                if (_slider != null)
                    _slider.value = val;
            })
            .AddTo(_disposables);

        _model.CurrentDashCount
            .Subscribe(count =>
            {
                if (_slider != null)
                    _slider.gameObject.SetActive(count < _model.MaxDashChargeStack);
            })
            .AddTo(_disposables);
    }

    void OnDestroy()
    {
        _disposables.Dispose();
    }
}