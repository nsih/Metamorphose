// Assets/Scripts/UI/BossHealthBarView.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;
using System;

public class BossHealthBarView : MonoBehaviour
{
    [SerializeField] private Image _fillImage;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private TextMeshProUGUI _bossNameText;
    [SerializeField] private GameObject _barRoot;

    private CompositeDisposable _disposables = new CompositeDisposable();
    private BossController _currentBoss;

    private void Start()
    {
        _barRoot.SetActive(false);

        BossController boss = FindObjectOfType<BossController>();
        if (boss != null)
        {
            Bind(boss);
        }
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
        _disposables.Dispose();
    }
}