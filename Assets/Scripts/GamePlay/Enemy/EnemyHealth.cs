using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public event System.Action OnHit;
    public event System.Action OnDeath;

    private System.Action _returnToPool;

    [SerializeField] private EnemyDataSO enemyDataSO; 

    private int _currentHp;
    private bool _isDead = false;

    private void OnEnable()
    {
        if (enemyDataSO == null)
        {
            Debug.LogError($"{gameObject.name}: EnemyDataSO Null");
            return;
        }
        
        Initialize(enemyDataSO);
        _isDead = false;
    }


    public void Initialize(EnemyDataSO data)
    {
        enemyDataSO = data;
        _currentHp = enemyDataSO.MaxHp;
    }

    public void TakeDamage(int amount, Vector3 hitPoint)
    {
        if (_isDead) return;

        _currentHp -= amount;
        
        OnHit?.Invoke(); 

        if (_currentHp <= 0)
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
        
        OnDeath?.Invoke();
        // 사망 애니메이션 실행 및 대기
        // _animator.SetTrigger("Die");
        // await UniTask.Delay(TimeSpan.FromSeconds(1.5f)); // 효과음같은거
        // (옵션) 서서히 투명해지기 등 연출        
        

        //
        if (_returnToPool != null)
        {
            _returnToPool.Invoke(); 
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}