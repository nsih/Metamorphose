using UnityEngine;

[CreateAssetMenu(fileName = "Cond_Raycast", menuName = "SO/Enemy/Conditions/Raycast")]
public class RaycastConditionSO : TransitionConditionSO
{
    public enum RaycastTarget { Player, Forward }
    public enum ResultType { Clear, Blocked }
    
    [Header("Raycast Settings")]
    public RaycastTarget Target = RaycastTarget.Player;
    public LayerMask ObstacleLayer;
    
    [Header("Condition")]
    public ResultType RequiredResult = ResultType.Clear;
    
    [Header("Optional")]
    [Tooltip("0이면 타겟까지 거리만 체크")]
    public float MaxDistance = 0f;
    public float OriginOffset = 0.5f;
    
    [Header("Debug")]
    public bool ShowDebugRay = true;
    
    public override bool Evaluate(EnemyContext ctx)
    {
        if (Target == RaycastTarget.Player && ctx.Target == null)
            return false;
        
        Vector2 origin = ctx.Self.position;
        Vector2 direction;
        float distance;
        
        if (Target == RaycastTarget.Player)
        {
            Vector2 targetPos = ctx.Target.position;
            direction = (targetPos - origin).normalized;
            float distToTarget = Vector2.Distance(origin, targetPos);
            
            if (MaxDistance > 0)
                distance = Mathf.Min(MaxDistance, distToTarget);
            else
                distance = distToTarget;
        }
        else
        {
            direction = ctx.Self.right;
            distance = MaxDistance > 0 ? MaxDistance : 10f;
        }
        
        Vector2 offsetOrigin = origin + direction * OriginOffset;
        float adjustedDistance = distance - OriginOffset;
        
        if (adjustedDistance <= 0)
            return RequiredResult == ResultType.Clear;
        
        RaycastHit2D hit = Physics2D.Raycast(offsetOrigin, direction, adjustedDistance, ObstacleLayer);
        bool isBlocked = hit.collider != null;
        
        if (ShowDebugRay)
        {
            Color rayColor = isBlocked ? Color.red : Color.green;
            Debug.DrawRay(offsetOrigin, direction * adjustedDistance, rayColor, 0.1f);
        }
        
        return RequiredResult == ResultType.Blocked ? isBlocked : !isBlocked;
    }
}