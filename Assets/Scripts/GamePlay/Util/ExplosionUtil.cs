using UnityEngine;
using Common;

public static class ExplosionUtil
{
    public static void Explode(Vector3 center, ExplosionOwner owner, ExplosionConfigSO config)
    {
        if (config == null)
        {
            Debug.LogWarning("ExplosionUtil: config null");
            return;
        }
        
        Explode(center, owner, config.Radius, config.Damage, config.Force, config.TargetLayer);
        SpawnVFX(center, config.VFXPrefab);
    }
    
    public static void Explode(
        Vector3 center,
        ExplosionOwner owner,
        float radius,
        float damage,
        float force = 0f,
        LayerMask targetLayer = default)
    {
        if (targetLayer == default)
            targetLayer = ~0;
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, targetLayer);
        
        foreach (var hit in hits)
        {
            ProcessHit(hit, owner, center, damage, force);
        }
        
        DrawDebug(center, radius);
    }
    
    private static void ProcessHit(
        Collider2D hit, 
        ExplosionOwner owner, 
        Vector3 center, 
        float damage, 
        float force)
    {
        // 1. IExplosionReactable
        var reactables = hit.GetComponents<IExplosionReactable>();
        if (reactables.Length > 0)
        {
            foreach (var reactable in reactables)
            {
                reactable.OnExplosion(owner, center, damage, force);
            }
            return;
        }
        
        // 2. BulletPro
        var bullet = hit.GetComponent<BulletPro.Bullet>();
        if (bullet != null)
        {
            HandleBullet(bullet, owner, hit.gameObject.layer);
            return;
        }
        
        // 3. IDamageable
        var damageable = hit.GetComponent<IDamageable>();
        if (damageable != null)
        {
            if (ShouldDamage(hit.gameObject.layer, owner))
            {
                damageable.TakeDamage((int)damage);
            }
        }
    }
    
    private static void HandleBullet(BulletPro.Bullet bullet, ExplosionOwner owner, int bulletLayer)
    {
        string layerName = LayerMask.LayerToName(bulletLayer);
        
        bool shouldDestroy = owner switch
        {
            ExplosionOwner.Player => layerName == "EnemyBullet",
            ExplosionOwner.Enemy => layerName == "PlayerBullet",
            ExplosionOwner.Environment => true,
            _ => false
        };
        
        if (shouldDestroy)
        {
            bullet.Die();
        }
    }
    
    private static bool ShouldDamage(int targetLayer, ExplosionOwner owner)
    {
        string layerName = LayerMask.LayerToName(targetLayer);
        
        return owner switch
        {
            ExplosionOwner.Player => layerName is "Enemy" or "Destructible",
            ExplosionOwner.Enemy => layerName is "Player" or "Destructible",
            ExplosionOwner.Environment => true,
            _ => false
        };
    }
    
    private static void SpawnVFX(Vector3 position, GameObject vfxPrefab)
    {
        if (vfxPrefab != null)
        {
            Object.Instantiate(vfxPrefab, position, Quaternion.identity);
        }
    }
    
    private static void DrawDebug(Vector3 center, float radius)
    {
        #if UNITY_EDITOR
        Debug.DrawRay(center + Vector3.up * radius, Vector3.down * radius * 2, Color.red, 0.5f);
        Debug.DrawRay(center + Vector3.left * radius, Vector3.right * radius * 2, Color.red, 0.5f);
        #endif
    }
}