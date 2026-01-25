using UnityEngine;
using Cysharp.Threading.Tasks;

[CreateAssetMenu(fileName = "Death_None", menuName = "SO/Enemy/DeathEffects/None")]
public class NoneDeathEffectSO : DeathEffectSO
{
    public override UniTask Execute(EnemyContext ctx)
    {
        return UniTask.CompletedTask;
    }
}