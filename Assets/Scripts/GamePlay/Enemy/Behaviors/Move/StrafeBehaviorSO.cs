using UnityEngine;

[CreateAssetMenu(fileName = "Move_Strafe", menuName = "SO/Enemy/Behaviors/Move/Strafe")]
public class StrafeBehaviorSO : EnemyMoveBehaviorSO
{
    [Header("Strafe Settings")]
    [Tooltip("방향 전환 간격")]
    public float DirectionChangeInterval = 1.5f;
    
    [Tooltip("공격 후 정지 시간")]
    public float StopDurationAfterAttack = 0.5f;
    
    private static readonly int DirXKey = "StrafeDirX".GetHashCode();
    private static readonly int DirYKey = "StrafeDirY".GetHashCode();
    private static readonly int TimerKey = "StrafeDirTimer".GetHashCode();
    
    public override void Execute(EnemyContext ctx)
    {
        if (ctx.IsDead) return;
        if (ctx.Target == null) return;
        
        float timeSinceAttack = Time.time - ctx.LastAttackTime;
        
        if (timeSinceAttack < StopDurationAfterAttack) return;
        if (timeSinceAttack >= ctx.CurrentAttackCoolTime) return;
        
        float timer = ctx.GetFloat(TimerKey);
        timer += Time.deltaTime;
        
        float dirX = ctx.GetFloat(DirXKey);
        float dirY = ctx.GetFloat(DirYKey);
        
        if ((dirX == 0 && dirY == 0) || timer >= DirectionChangeInterval)
        {
            timer = 0f;
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            dirX = Mathf.Cos(angle);
            dirY = Mathf.Sin(angle);
        }
        
        ctx.SetFloat(TimerKey, timer);
        ctx.SetFloat(DirXKey, dirX);
        ctx.SetFloat(DirYKey, dirY);
        
        Vector3 moveDir = new Vector3(dirX, dirY, 0).normalized;
        ctx.Self.position += moveDir * ctx.CurrentMoveSpeed * Time.deltaTime;
    }
}