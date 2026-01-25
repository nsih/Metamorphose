using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using BulletPro;

[CreateAssetMenu(fileName = "Death_Shoot", menuName = "SO/Enemy/DeathEffects/Shoot")]
public class ShootOnDeathEffectSO : DeathEffectSO
{
    public EmitterProfile Pattern;
    
    [Tooltip("총알이 발사될 시간 확보")]
    public float DelayBeforeDisable = 0.1f;
    
    public override async UniTask Execute(EnemyContext ctx)
    {
        if (ctx.Emitter != null && Pattern != null)
        {
            ctx.Emitter.emitterProfile = Pattern;
            ctx.Emitter.Play();
            
            await UniTask.Delay(TimeSpan.FromSeconds(DelayBeforeDisable));
        }
    }
}