using UnityEngine;
using BulletPro;
using Cysharp.Threading.Tasks;


[RequireComponent(typeof(BulletReceiver))]
public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyDataSO _data;
    [SerializeField] private BulletReceiver _receiver;

    public event System.Action OnHit;
    public event System.Action OnDeath;
    private System.Action _returnToPool;

    private int _hp;
    private bool _isDead = false;

    private void Awake()
    {
        _receiver = GetComponent<BulletReceiver>();
    }

    private void OnEnable()
    {
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

    public void Initialize(EnemyDataSO data)
    {
        _data = data;
        _hp = _data.MaxHp;
        _isDead = false;
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
        
        // Debug.Log($"{gameObject.name} HP: {_hp}");
        
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
        OnDeath?.Invoke();

        // 

        if (_returnToPool != null) _returnToPool.Invoke();
        else gameObject.SetActive(false);
    }
}