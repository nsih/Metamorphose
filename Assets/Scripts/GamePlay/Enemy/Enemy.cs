using UnityEngine;
using BulletPro;
using Cysharp.Threading.Tasks;

// 1. 상태 정의 (가벼운 FSM)
public enum EnemyState
{
    Idle,   // 대기/배회
    Chase,  // 추적
    Attack  // 공격
}

[RequireComponent(typeof(BulletReceiver))]
[RequireComponent(typeof(EnemyMovement))] // Movement 필수 보장
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

    private void Awake()
    {
        _receiver = GetComponent<BulletReceiver>();
        _movement = GetComponent<EnemyMovement>(); // 다리(이동) 컴포넌트 가져오기
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
                // 공격 범위를 벗어나면 -> 다시 추적
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
        // 쿨타임 체크
        if (Time.time - _lastAttackTime >= _data.AttackCoolTime)
        {
            _lastAttackTime = Time.time;
            PerformAttack().Forget();
        }
    }

    private async UniTaskVoid PerformAttack()
    {
        // TODO: 나중에 AttackStrategySO를 연결할 곳
        Debug.Log("Attack"); 
        
        // 임시 딜레이 (애니메이션 시간)
        await UniTask.Delay(500); 
    }
    // -----------------------

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
            damage = bullet.moduleParameters.GetFloat("_Damage");
            if (damage == 0) damage = 1;
        }

        TakeDamage((int)damage);
        bullet.Die();
    }

    public void TakeDamage(int dmg)
    {
        if (_isDead) return;

        _hp -= dmg;
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
}