using UnityEngine;
using Reflex.Attributes;
using BulletPro;

[RequireComponent(typeof(BulletReceiver))]
public class PlayerHitManager : MonoBehaviour
{
    [Inject] private PlayerModel _model;

    [SerializeField] private BulletReceiver _receiver;

    private PlayerDash _playerDash;
    private BulletTimeManager _bulletTimeManager;


    public bool IsInvincible => (_playerDash != null && _playerDash.IsDashing);

    void Awake()
    {
        _receiver = GetComponent<BulletReceiver>();
        _playerDash = GetComponent<PlayerDash>();
        _bulletTimeManager = GetComponent<BulletTimeManager>();
    }

    void Start()
    {
        if (_model == null)
        {
            Debug.LogError("PlayerHitManager: DI Error");
            return;
        }

        if (_receiver != null)
        {
            _receiver.OnHitByBullet.AddListener(HandleBulletHit);
        }
    }

    public void HandleBulletHit(Bullet bullet, Vector3 hitPoint)
    {
        // 이미 죽었으면 무시 (Model 데이터 확인)
        if (_model.CurrentHP.Value <= 0) return;
        
        if (IsInvincible)
        {
            Debug.Log("Graze");
            
            if (_bulletTimeManager != null)
            {
                _bulletTimeManager.TriggerSlowMotion();
            }
            
            // 그레이즈 시 bullet.Die() 호출 안 함
            return; 
        }

        float damage = 1; // 기본 데미지
        if (bullet.dynamicSolver != null)
        {
            damage = bullet.moduleParameters.GetFloat("_Damage");
            if (damage == 0) damage = 1; 
        }

        Debug.Log($"dmg: {damage}");
        
        _model.TakeDamage(damage);

        bullet.Die();
        
        if (_model.CurrentHP.Value <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Game Over");
        
        // 임시 처리
        gameObject.SetActive(false); 
    }
}