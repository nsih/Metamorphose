using UnityEngine;
using Reflex.Attributes;
using BulletPro;


[RequireComponent(typeof(BulletReceiver))]
public class PlayerHitManager : MonoBehaviour
{
    [Inject]
    private PlayerStat _playerStat;

    [SerializeField] private BulletReceiver _receiver;

    // 내부 상태 변수
    private float _currentHp;
    private bool _isDead = false;
    
    // [내일 구현 예정] 무적 상태 플래그
    // private bool _isInvincible = false; 


    void Start()
    {
        // 의존성 주입 확인
        if (_playerStat == null)
        {
            Debug.LogError("PlayerHitManager: PlayerStat 데이터 주입 실패");
            return;
        }

        InitializeHealth();

        //if (_receiver == null) _receiver = GetComponent<BulletReceiver>();
        _receiver.OnHitByBullet.AddListener(TakeDamage);
    }

    private void InitializeHealth()
    {
        _currentHp = _playerStat.MaxHealth;
        Debug.Log($"InitializeHealth : {_currentHp}/{_playerStat.MaxHealth}");
    }


    public void TakeDamage(Bullet bullet, Vector3 hitPoint)
    {
        Debug.Log("taking damage");
        if (_isDead) return;
        
        // if (_isInvincible) return; // 내일 무적 로직 추가

        float damage = 0; 
        if (bullet.dynamicSolver != null)
        {
            damage = bullet.moduleParameters.GetFloat("_Damage");
        }

        _currentHp -= damage;
        Debug.Log(_currentHp);

        // 사망 체크
        if (_currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        _isDead = true;

        // 임시 처리: 플레이어 비활성화
        gameObject.SetActive(false); 
    }
}
