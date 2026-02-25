using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Reflex.Attributes;
using R3;
using System;

public class PlayerStatTestView : MonoBehaviour
{
    [Header("HP UI")]
    [SerializeField] private TextMeshProUGUI _hpText;

    [Header("Dash UI")]
    [SerializeField] private TextMeshProUGUI _dashCountText;
    [SerializeField] private Slider _dashSlider;

    private PlayerModel _model;
    private CompositeDisposable _disposables = new CompositeDisposable();

    [Inject]
    public void Construct(PlayerModel model)
    {
        _model = model;
    }

    private void Start()
    {
        if (_model == null) return;

        _model.CurrentHP
            .Subscribe(hp => _hpText.text = $"HP: {hp} / {_model.MaxHP}")
            .AddTo(_disposables);

        _model.CurrentDashCount
            .Subscribe(count => _dashCountText.text = $"Dash: {count}")
            .AddTo(_disposables);

        _model.DashCooldownNormalized
            .Subscribe(val => _dashSlider.value = val)
            .AddTo(_disposables);
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }
}