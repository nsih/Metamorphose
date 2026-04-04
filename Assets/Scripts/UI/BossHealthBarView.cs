using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;
using Reflex.Attributes;

public class BossHealthBarView : MonoBehaviour
{
    [SerializeField] private Image _fillImage;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private TextMeshProUGUI _bossNameText;
    [SerializeField] private GameObject _barRoot;

    private ReactiveProperty<BossController> _bossHandle;

    private CompositeDisposable _outerDisposables = new CompositeDisposable();
    private CompositeDisposable _disposables = new CompositeDisposable();

    private BossController _currentBoss;

    [Inject]
    public void Construct(ReactiveProperty<BossController> bossHandle)
    {
        _bossHandle = bossHandle;
    }

    private void Start()
    {
        _barRoot.SetActive(false);

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
                    Bind(boss);
                }
                else
                {
                    Hide();
                }
            })
            .AddTo(_outerDisposables);
    }

    private void Bind(BossController boss)
    {
        if (_currentBoss != null)
        {
            Hide();
        }

        _currentBoss = boss;
        _bossNameText.text = boss.Profile.BossNameKr;

        int maxHP = Mathf.Max(1, boss.MaxHP);

        boss.CurrentHP
            .Subscribe(hp => _fillImage.fillAmount = (float)hp / maxHP)
            .AddTo(_disposables);

        boss.CurrentPhaseIndex
            .Subscribe(phase => Debug.Log($"boss phase ui: {phase}"))
            .AddTo(_disposables);

        boss.OnBossDeath += Hide;
        _barRoot.SetActive(true);
    }

    public void Hide()
    {
        _disposables.Dispose();
        _disposables = new CompositeDisposable();

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
        _disposables.Dispose();
    }
}