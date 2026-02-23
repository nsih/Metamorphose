using UnityEngine;
using Cysharp.Threading.Tasks;
using Common;

[CreateAssetMenu(fileName = "Death_Explode", menuName = "SO/Enemy/DeathEffects/Explode")]
public class ExplodeOnDeathEffectSO : DeathEffectSO
{
    [SerializeField] private ExplosionConfigSO explosionConfig;
    
    public override UniTask Execute(EnemyContext ctx)
    {
        if (explosionConfig != null)
        {
            ExplosionUtil.Explode(ctx.Self.position, ExplosionOwner.Enemy, explosionConfig);
        }
        
        return UniTask.CompletedTask;
    }
}