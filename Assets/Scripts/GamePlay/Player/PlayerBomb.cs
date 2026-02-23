using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using Reflex.Attributes;
using Common;
using BulletPro;

public class PlayerBomb : MonoBehaviour
{
    [Inject] private IInputService _input;
    
    [SerializeField] private ExplosionConfigSO config;
    [SerializeField] private float duration = 3f;
    [SerializeField] private float tickInterval = 0.1f;
    
    private bool _isActive;
    private CancellationTokenSource _cts;
    
    public bool IsActive => _isActive;

    private void Start()
    {
        if (_input != null)
        {
            _input.OnBombPressed += Activate;
        }
    }

    private void OnDestroy()
    {
        if (_input != null)
        {
            _input.OnBombPressed -= Activate;
        }
        Cancel();
    }

    public void Activate()
    {
        if (_isActive) return;
        
        Cancel();
        _cts = new CancellationTokenSource();
        BombSequence(_cts.Token).Forget();
    }

    private void Cancel()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
        _isActive = false;
    }

    private async UniTaskVoid BombSequence(CancellationToken token)
    {
        _isActive = true;
        
        float elapsed = 0f;
        
        // 첫 틱: 적에게 데미지
        ExplosionUtil.Explode(transform.position, ExplosionOwner.Player, config);
        
        while (elapsed < duration)
        {
            if (token.IsCancellationRequested) break;
            
            ClearBulletsInRange();
            
            await UniTask.Delay((int)(tickInterval * 1000), cancellationToken: token);
            elapsed += tickInterval;
        }
        
        _isActive = false;
    }

    private void ClearBulletsInRange()
    {
        var manager = BulletPro.BulletPoolManager.instance;
        if (manager == null) return;
        
        float radiusSqr = config.Radius * config.Radius;
        Vector3 center = transform.position;
        
        foreach (var bullet in manager.pool)
        {
            if (bullet == null) continue;
            if (!bullet.gameObject.activeInHierarchy) continue;
            
            string layerName = LayerMask.LayerToName(bullet.self.gameObject.layer);
            if (layerName != "EnemyBullet") continue;
            
            float distSqr = (bullet.self.position - center).sqrMagnitude;
            if (distSqr <= radiusSqr)
            {
                bullet.Die();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (config == null) return;
        
        Gizmos.color = _isActive ? Color.cyan : Color.gray;
        Gizmos.DrawWireSphere(transform.position, config.Radius);
    }
}