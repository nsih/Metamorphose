using UnityEngine;
using Cysharp.Threading.Tasks;

public abstract class DeathEffectSO : ScriptableObject
{
    public abstract UniTask Execute(EnemyContext ctx);
}