// Assets/Scripts/UI/BossHealthBarView.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;
using Reflex.Attributes;
using Cysharp.Threading.Tasks;

public class BossHealthBarView : MonoBehaviour
{
    [SerializeField] private Image _fillImage;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private TextMeshProUGUI _bossNameText;
    [SerializeField] private GameObject _barRoot;

    private ReactiveProperty<BossController> _bossHandle;

    private readonly CompositeDisposable _outerDisposables = new CompositeDisposable();
    private readonly SerialDisposable _bindSub = new SerialDisposable();

    private BossController _currentBoss;

    [Inject]
    public void Construct(ReactiveProperty<BossController> bossHandle)
    {
        _bossHandle = bossHandle;
    }

    private void Start()
    {
        _barRoot.SetActive(false);
        _bindSub.AddTo(_outerDisposables);

        if (_bossHandle == null)
        {
            Debug.LogWarning("BossHealthBarView: bossHandle null");
            return;
        }

        _bossHandle
            .Subscribe(boss =>
            {
                if (boss != null)
                {
                    BindDelayed(boss).Forget();
                }
                else
                {
                    Hide();
                }
            })
            .AddTo(_outerDisposables);
    }

    private async UniTaskVoid BindDelayed(BossController boss)
    {
        // BossController.Start()에서 _ctx 초기화 대기
        await UniTask.DelayFrame(1);

        if (this == null) return;
        if (boss == null) return;

        Bind(boss);
    }

    public void Bind(BossController boss)
    {
        if (_currentBoss != null)
        {
            Hide();
        }

        _currentBoss = boss;
        _bossNameText.text = boss.Profile.BossNameKr;

        int maxHP = Mathf.Max(1, boss.MaxHP);

        var innerSub = new CompositeDisposable();

        boss.CurrentHP
            .Subscribe(hp => _fillImage.fillAmount = (float)hp / maxHP)
            .AddTo(innerSub);

        boss.CurrentPhaseIndex
            .Subscribe(phase => Debug.Log($"boss phase ui: {phase}"))
            .AddTo(innerSub);

        _bindSub.Disposable = innerSub;

        boss.OnBossDeath += Hide;
        _barRoot.SetActive(true);
    }

    public void Hide()
    {
        _bindSub.Disposable = Disposable.Empty;

        if (_currentBoss != null)
        {
            _currentBoss.OnBossDeath -= Hide;
            _currentBoss = null;
        }

        _barRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        _outerDisposables.Dispose();
    }
}