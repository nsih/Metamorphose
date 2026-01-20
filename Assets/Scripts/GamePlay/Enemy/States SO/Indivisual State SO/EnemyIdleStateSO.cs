using UnityEngine;
using Common;

[CreateAssetMenu(fileName = "EnemyIdleState", menuName = "SO/Enemy/States/Idle")]
public class EnemyIdleStateSO : EnemyStateSO
{
    public override EnemyState StateType => EnemyState.Idle;

    public override void Enter(EnemyContext ctx)
    {
        
    }

    public override void Execute(EnemyContext ctx)
    {
        
    }

    public override void Exit(EnemyContext ctx)
    {
        
    }

    public override void Reset(EnemyContext ctx)
    {
        
    }
}