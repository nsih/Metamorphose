using UnityEngine;

[CreateAssetMenu(fileName = "Move_Strafe", menuName = "SO/Enemy/Behaviors/Move/Strafe")]
public class StrafeBehaviorSO : EnemyMoveBehaviorSO
{
    [Header("Strafe Settings")]
    public float StrafeRange = 3f;
    public float StrafeSpeed = 1f;
    
    [Header("Distance Control")]
    [Tooltip("유지하려는 거리")]
    public float PreferredDistance = 15;
    
    [Tooltip("거리 허용 오차")]
    public float DistanceTolerance = 5f;
    
    [Tooltip("전후 이동 강도")]
    [Range(0f, 1f)]
    public float DistanceCorrectionStrength = 0.3f;
    
    public override void Execute(EnemyContext ctx)
    {
        if (ctx.IsDead) return;
        if (ctx.Target == null) return;
        if (ctx.Rigidbody == null) return;
        
        float time = ctx.GetFloat(EnemyContextKeys.StrafeTime);
        time += Time.deltaTime;
        ctx.SetFloat(EnemyContextKeys.StrafeTime, time);
        
        Vector2 toPlayer = ctx.Target.position - ctx.Self.position;
        float currentDistance = toPlayer.magnitude;
        Vector2 dirToPlayer = toPlayer.normalized;
        
        Vector2 right = new Vector2(-dirToPlayer.y, dirToPlayer.x);
        
        float strafeOffset = Mathf.Sin(time * StrafeSpeed) * StrafeRange;
        Vector2 strafeVelocity = right * strafeOffset;
        
        float distanceError = currentDistance - PreferredDistance;
        Vector2 distanceCorrection = Vector2.zero;
        
        if (Mathf.Abs(distanceError) > DistanceTolerance)
        {
            distanceCorrection = -dirToPlayer * distanceError * DistanceCorrectionStrength;
        }
        
        Vector2 finalVelocity = strafeVelocity + distanceCorrection;
        ctx.Rigidbody.linearVelocity = finalVelocity;
    }
}