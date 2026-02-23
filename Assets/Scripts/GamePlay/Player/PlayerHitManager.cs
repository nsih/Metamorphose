using UnityEngine;
using Reflex.Attributes;
using BulletPro;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(BulletReceiver))]
public class PlayerHitManager : MonoBehaviour
{
    [Inject] private PlayerModel _model;

    private BulletReceiver _receiver;
    private SpriteRenderer _spriteRenderer;


    private PlayerBomb _playerBomb;
    private PlayerDash _playerDash;
    private BulletTimeManager _bulletTimeManager;


    //public bool IsInvincible => (_playerDash != null && _playerDash.IsDashing);
    public bool IsInvincible => 
    (_playerDash != null && _playerDash.IsDashing) || 
    (_playerBomb != null && _playerBomb.IsActive);

    void Awake()
    {
        _receiver = GetComponent<BulletReceiver>();
        _playerDash = GetComponent<PlayerDash>();
        _playerBomb = GetComponent<PlayerBomb>();
        _bulletTimeManager = GetComponent<BulletTimeManager>();

        _spriteRenderer = GetComponent<SpriteRenderer>();
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
        
        //그레이즈 처리
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

        //Debug.Log($"dmg: {damage}");
        
        _model.TakeDamage(damage);
        PlayHitFeedback().Forget();

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

    //
    private async UniTaskVoid PlayHitFeedback()
    {
        //맞을때 잠깐 번쩍이는데 무적시간 추가할때 이것도 바꿔야함
        if (_spriteRenderer != null)
        {
            var token = this.GetCancellationTokenOnDestroy();
            try
            {
                _spriteRenderer.color = Color.red;
                await UniTask.Delay(100, cancellationToken: token);
                _spriteRenderer.color = Color.white;
            }
            catch (System.OperationCanceledException) { }
        }

        // 카메라 흔드는 효과 추가? (아직 안만듬)
    }
}