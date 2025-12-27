using UnityEngine;
using BulletPro;
using Cysharp.Threading.Tasks;
using Common;


[RequireComponent(typeof(BulletReceiver))]
[RequireComponent(typeof(EnemyMovement))]
public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyDataSO _data;
    [SerializeField] private BulletReceiver _receiver;

    public event System.Action OnHit;
    public event System.Action OnDeath;
    System.Action _returnToPool;

    int _hp;
    bool _isDead = false;

    private Transform _target;

    // FSM 관련 변수
    private EnemyState _currentState;
    private EnemyMovement _movement;
    private float _lastAttackTime;

    //sr
    private SpriteRenderer _spriteRenderer;



    private void Awake()
    {
        _receiver = GetComponent<BulletReceiver>();
        _movement = GetComponent<EnemyMovement>(); // 다리(이동) 컴포넌트 가져오기
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        // 데이터가 있다면 초기화, 없으면 나중에 Factory가 해줌
        if (_data != null) Initialize(_data);
        
        _isDead = false;

        if (_receiver != null)
        {
            _receiver.OnHitByBullet.AddListener(OnBulletHit);
        }
    }

    private void OnDisable()
    {
        if (_receiver != null)
        {
            _receiver.OnHitByBullet.RemoveListener(OnBulletHit);
        }
    }
    
    // 2. 뇌 가동 (Update Loop)
    private void Update()
    {
        // 죽었거나 타겟이 없으면 아무것도 안 함
        if (_isDead || _target == null) 
        {
            StopMoving();
            return;
        }

        // 거리 측정
        float distance = GetDistanceToTarget();

        // 상태 기계 (State Machine)
        switch (_currentState)
        {
            case EnemyState.Idle:
                // 감지 범위 안에 들어오면 -> 추적 시작
                if (distance <= _data.AggroRange)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;

            case EnemyState.Chase:
                // 공격 범위 안에 들어오면 -> 공격 태세 (멈춤)
                if (distance <= _data.AttackRange)
                {
                    ChangeState(EnemyState.Attack);
                }
                // 감지 범위를 벗어나면 -> 다시 대기 (놓침)
                else if (distance > _data.AggroRange)
                {
                    ChangeState(EnemyState.Idle);
                }
                break;

            case EnemyState.Attack:
                // 공격 범위를 벗어나면 -> 다시 이동
                if (distance > _data.AttackRange)
                {
                    ChangeState(EnemyState.Chase);
                }
                else
                {
                    // 범위 안이라면 계속 공격 시도
                    TryAttack();
                }
                break;
        }
    }

    // 상태 전환 처리
    private void ChangeState(EnemyState newState)
    {
        _currentState = newState;

        switch (_currentState)
        {
            case EnemyState.Idle:
                StopMoving(); // 대기 상태에선 움직임 끔
                break;
            case EnemyState.Chase:
                ResumeMoving(); // 추적 상태에선 움직임 켬 (전략 패턴 가동)
                break;
            case EnemyState.Attack:
                StopMoving(); // 공격 할 땐 멈춰야 함 (무빙샷 적이라면 여기 수정)
                break;
        }
    }

    public void Initialize(EnemyDataSO data)
    {
        _data = data;
        _hp = _data.MaxHp;
        _isDead = false;
        
        // 초기 상태는 Idle
        _currentState = EnemyState.Idle;
        _lastAttackTime = 0f;
        StopMoving();
    }

    // --- 행동 제어 메서드 ---
    private void ResumeMoving()
    {
        if (_movement != null) _movement.enabled = true;
    }

    private void StopMoving()
    {
        if (_movement != null) _movement.enabled = false;
    }

    private void TryAttack()
    {
        if (Time.time - _lastAttackTime >= _data.AttackCoolTime)
        {
            _lastAttackTime = Time.time;
            PerformAttack().Forget();
        }
    }

    private async UniTaskVoid PerformAttack()
    {
        if (_data.AttackStrategy == null) return;

        _data.AttackStrategy.Attack(transform, _target);
        
        // 후딜
        int delay = 1000;
        await UniTask.Delay(delay, cancellationToken: this.GetCancellationTokenOnDestroy()); 
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public float GetDistanceToTarget()
    {
        if (_target == null) return float.MaxValue;
        return Vector3.Distance(transform.position, _target.position);
    }

    void OnDrawGizmosSelected()
    {
        if (_data == null) return;
        
        // 어그로 범위 (노랑)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _data.AggroRange);

        // 공격 범위 (빨강)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _data.AttackRange);
        
        // 현재 상태 표시 (씬 뷰 위에 텍스트로 띄우면 좋지만 일단 패스)
    }

    public void OnBulletHit(Bullet bullet, Vector3 hitPoint)
    {
        if (_isDead) return;

        float damage = 1f;

        if (bullet.dynamicSolver != null)
        {
            damage = bullet.moduleParameters.GetFloat("Damage");
            if (damage == 0) damage = 1;
        }

        TakeDamage((int)damage);
        bullet.Die();
    }

    private async UniTaskVoid PlayFlashEffect()
    {
        if (_spriteRenderer == null) return;
        var token = this.GetCancellationTokenOnDestroy();

        try
        {
            _spriteRenderer.color = Color.red;
            await UniTask.Delay(100, cancellationToken: token);

            _spriteRenderer.color = Color.white; 
        }catch (System.OperationCanceledException){}
    }

    public void TakeDamage(int dmg)
    {
        if (_isDead) return;

        _hp -= dmg;
        PlayFlashEffect().Forget();
        
        OnHit?.Invoke();

        if (_hp <= 0)
        {
            DieSequence().Forget();
        }
    }

    public void SetReleaseAction(System.Action returnToPool)
    {
        _returnToPool = returnToPool;
    }

    private async UniTaskVoid DieSequence()
    {
        _isDead = true;
        StopMoving(); // 죽으면 멈춰야 함
        OnDeath?.Invoke();

        if (_returnToPool != null) _returnToPool.Invoke();
        else gameObject.SetActive(false);
    }


    #if UNITY_EDITOR
    private void OnGUI()
    {
        if (_target == null || _isDead) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.8f); // 0.8f는 높이 조절
        if (screenPos.z < 0) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 15;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        
        switch (_currentState)
        {
            case EnemyState.Idle: style.normal.textColor = Color.green; break;
            case EnemyState.Chase: style.normal.textColor = Color.yellow; break;
            case EnemyState.Attack: style.normal.textColor = Color.red; break;
        }

        screenPos.y = Screen.height - screenPos.y;

        string text = $"{_currentState}\nDist: {GetDistanceToTarget():F1}";
        
        GUI.Label(new Rect(screenPos.x - 50, screenPos.y - 25, 100, 50), text, style);
    }
#endif
}