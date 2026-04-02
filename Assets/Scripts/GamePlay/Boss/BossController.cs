// Assets/Scripts/GamePlay/Boss/BossController.cs
using System;
using UnityEngine;
using BulletPro;
using Cysharp.Threading.Tasks;
using R3;

[RequireComponent(typeof(EnemyFSM))]
public class BossController : MonoBehaviour
{
    [SerializeField] private BossProfileSO _profile;

    private EnemyFSM _fsm;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rb;
    private BulletReceiver _receiver;
    private BulletEmitter _emitter;
    private EnemyMuzzleAim _muzzleAim;

    private BossContext _ctx;
    private Transform _target;
    private bool _targetSetExternally = false;

    public ReactiveProperty<int> CurrentHP => _ctx.CurrentHP;
    public ReactiveProperty<int> CurrentPhaseIndex => _ctx.CurrentPhaseIndex;
    public int MaxHP => _profile.TotalMaxHP;
    public BossProfileSO Profile => _profile;
    public event Action OnBossDeath;

    private void Awake()
    {
        _fsm = GetComponent<EnemyFSM>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _receiver = GetComponent<BulletReceiver>();
        _emitter = GetComponentInChildren<BulletEmitter>();
        _muzzleAim = GetComponentInChildren<EnemyMuzzleAim>();
    }

    // RoomManager에서 Instantiate 직후 호출 — 비활성 플레이어 문제 우회
    public void SetTarget(Transform target)
    {
        _target = target;
        _targetSetExternally = true;
    }

    private void Start()
    {
        if (!_profile.Validate())
        {
            Debug.LogError("BossController: profile 검증 실패");
            return;
        }

        _ctx = new BossContext(_profile, transform, _spriteRenderer, _emitter, _rb);

        // 외부에서 타겟 미전달 시 씬 탐색 (BossTest씬 fallback)
        if (!_targetSetExternally)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            _target = playerObj != null ? playerObj.transform : null;

            if (_target == null)
            {
                Debug.LogWarning("BossController: Player 태그 없음");
            }
        }

        BossPhaseSO firstPhase = _profile.Phases[0];
        _ctx.InnerContext.Initialize(firstPhase.Brain, _target);
        _muzzleAim?.SetTarget(_target);
        _fsm.Initialize(firstPhase.Brain, _ctx.InnerContext);
    }

    private void OnEnable()
    {
        _receiver?.OnHitByBullet.AddListener(OnBulletHit);
    }

    private void OnDisable()
    {
        _receiver?.OnHitByBullet.RemoveListener(OnBulletHit);
    }

    private void OnBulletHit(Bullet bullet, Vector3 hitPoint)
    {
        if (_ctx == null || _ctx.IsDead) return;
        if (_ctx.IsInvulnerable) return;

        float damage = bullet.moduleParameters.GetFloat("_Damage");
        if (damage == 0f) damage = 1f;

        TakeDamage((int)damage);
        bullet.Die();
    }

    private void TakeDamage(int dmg)
    {
        _ctx.CurrentHP.Value = Mathf.Max(0, _ctx.CurrentHP.Value - dmg);

        if (_ctx.CurrentHP.Value <= 0)
        {
            DieSequence().Forget();
            return;
        }

        CheckPhaseTransition();
    }

    private void CheckPhaseTransition()
    {
        int newPhase = _profile.GetPhaseIndex(_ctx.HPRatio);

        if (newPhase != _ctx.CurrentPhaseIndex.Value)
        {
            _fsm.Stop();
            _ctx.CurrentPhaseIndex.Value = newPhase;
            TransitionToPhase(newPhase).Forget();
        }
    }

    private async UniTaskVoid TransitionToPhase(int phaseIndex)
    {
        BossPhaseSO phase = _profile.Phases[phaseIndex];

        _ctx.IsInvulnerable = phase.IsInvulnerableDuringIntro;

        if (phase.IntroDelay > 0f)
        {
            await UniTask.Delay((int)(phase.IntroDelay * 1000), ignoreTimeScale: true);
        }

        _ctx.IsInvulnerable = false;

        _ctx.InnerContext.Reset();
        _ctx.InnerContext.Initialize(phase.Brain, _target);
        _fsm.Initialize(phase.Brain, _ctx.InnerContext);

        Debug.Log($"boss phase {phaseIndex}: {phase.PhaseName}");
    }

    private async UniTaskVoid DieSequence()
    {
        _ctx.IsDead = true;
        _fsm.Stop();

        if (_profile.DeathDelay > 0f)
        {
            await UniTask.Delay((int)(_profile.DeathDelay * 1000), ignoreTimeScale: true);
        }

        if (_profile.DeathEffect != null)
        {
            await _profile.DeathEffect.Execute(_ctx.InnerContext);
        }

        OnBossDeath?.Invoke();
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _ctx?.Dispose();
    }
}