using Cysharp.Threading.Tasks.Linq;
using R3;
using Reflex.Attributes;
using TJR.Core.GamePlay.Service;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Slider용

public class PlayerStatTestView : MonoBehaviour
{
    [Header("HP UI")]
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private TMP_Text _goldText;

    [Header("Dash UI")]
    [SerializeField] private TextMeshProUGUI _dashCountText; // 대시 횟수 (3, 2, 1...)
    [SerializeField] private Slider _dashSlider;             // 쿨타임 게이지 (0.0 ~ 1.0)

    [Inject] private PlayerModel _model;
    [Inject] private PlayerGoldService _goldService;

    void Start()
    {
        if (_model == null) return;

        _model.CurrentHP
            .Subscribe(hp => _hpText.text = $"HP: {hp} / {_model.MaxHP}")
            .AddTo(this);

        _model.CurrentDashCount
            .Subscribe(count => _dashCountText.text = $"Dash: {count}")
            .AddTo(this);

        _model.DashCooldownNormalized
            .Subscribe(val => _dashSlider.value = val)
            .AddTo(this);

        _goldService.Gold
            .AsObservable()
            .Subscribe(val => _goldText.text = $"Gold: {val}")
            .AddTo(this);
    }
}