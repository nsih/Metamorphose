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

    private ReactiveProperty<RoomManager> _roomHandle;

    // 방 전환 구독용
    private CompositeDisposable _outerDisposables = new CompositeDisposable();
    // 보스 HP/페이즈 구독용
    private CompositeDisposable _disposables = new CompositeDisposable();

    private BossController _currentBoss;

    [Inject]
    public void Construct(ReactiveProperty<RoomManager> roomHandle)
    {
        _roomHandle = roomHandle;
    }

    private void Start()
    {
        _barRoot.SetActive(false);

        if (_roomHandle != null)
        {
            // 방 전환 감지 — 보스방 진입 시 자동 바인딩
            _roomHandle
                .Subscribe(room => OnRoomChangedAsync(room).Forget())
                .AddTo(_outerDisposables);

            // 초기값이 null이면 씬에 미리 배치된 보스 탐색 (BossTest씬 대응)
            if (_roomHandle.Value == null)
            {
                TryAutoBindAsync().Forget();
            }
        }
        else
        {
            // DI 미연결 안전망
            TryAutoBindAsync().Forget();
        }
    }

    private async UniTaskVoid OnRoomChangedAsync(RoomManager room)
    {
        Hide();

        if (room == null) return;

        // 보스 Instantiate 및 BossController.Start() 완료 대기
        await UniTask.DelayFrame(1);

        // 오브젝트가 이미 파괴된 경우 방어
        if (this == null) return;

        BossController boss = FindObjectOfType<BossController>();

        if (boss != null)
        {
            Bind(boss);
        }
    }

    private async UniTaskVoid TryAutoBindAsync()
    {
        await UniTask.DelayFrame(1);

        if (this == null) return;

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
        _outerDisposables.Dispose();
        _disposables.Dispose();
    }
}