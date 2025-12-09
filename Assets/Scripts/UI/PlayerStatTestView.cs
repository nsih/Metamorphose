using UnityEngine;
using UnityEngine.UI; // Slider용
using TMPro;
using Reflex.Attributes;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

public class PlayerStatTestView : MonoBehaviour
{
    [Header("HP UI")]
    [SerializeField] private TextMeshProUGUI _hpText;

    [Header("Dash UI")]
    [SerializeField] private TextMeshProUGUI _dashCountText; // 대시 횟수 (3, 2, 1...)
    [SerializeField] private Slider _dashSlider;             // 쿨타임 게이지 (0.0 ~ 1.0)

    private PlayerModel _model;

    [Inject]
    public void Construct(PlayerModel model)
    {
        _model = model;
    }

    void Start()
    {
        if (_model == null) return;

        _model.CurrentHP
            .Subscribe(hp => _hpText.text = $"HP: {hp} / {_model.MaxHP}")
            .AddTo(this.GetCancellationTokenOnDestroy());

        _model.CurrentDashCount
            .Subscribe(count => _dashCountText.text = $"Dash: {count}")
            .AddTo(this.GetCancellationTokenOnDestroy());

        _model.DashCooldownNormalized
            .Subscribe(val => _dashSlider.value = val)
            .AddTo(this.GetCancellationTokenOnDestroy());
    }
}