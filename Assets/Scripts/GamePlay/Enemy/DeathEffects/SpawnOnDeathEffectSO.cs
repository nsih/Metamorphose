using UnityEngine;
using Cysharp.Threading.Tasks;

[CreateAssetMenu(fileName = "Death_Spawn", menuName = "SO/Enemy/DeathEffects/Spawn")]
public class SpawnOnDeathEffectSO : DeathEffectSO
{
    public EnemyBrainSO SpawnBrain;
    public int SpawnCount = 2;
    public float SpawnRadius = 1f;
    
    public override UniTask Execute(EnemyContext ctx)
    {
        if (ctx.SpawnAction == null || SpawnBrain == null) 
            return UniTask.CompletedTask;
        
        for (int i = 0; i < SpawnCount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * SpawnRadius;
            Vector3 spawnPos = ctx.Self.position + new Vector3(offset.x, offset.y, 0);
            ctx.SpawnAction.Invoke(spawnPos, SpawnBrain);
        }
        
        return UniTask.CompletedTask;
    }
}