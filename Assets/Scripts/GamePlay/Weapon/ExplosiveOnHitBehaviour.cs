using UnityEngine;
using BulletPro;
using Common;

public class ExplosiveOnHitBehaviour : BaseBulletBehaviour
{
    [SerializeField] private ExplosionConfigSO explosionConfig;
    [SerializeField] private ExplosionOwner owner = ExplosionOwner.Enemy;
    
    public override void OnBulletDeath()
    {
        base.OnBulletDeath();
        
        if (explosionConfig == null) return;
        
        Vector3 pos = bullet.self.position;
        ExplosionUtil.Explode(pos, owner, explosionConfig);
    }
}