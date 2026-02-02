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
        
        bool isVisible;
        
        if (Target == RaycastTarget.Player)
        {
            isVisible = ctx.CheckTargetVisibility(ObstacleLayer, MaxDistance, OriginOffset);
        }
        else
        {
            isVisible = CheckForward(ctx);
        }
        
        if (ShowDebugRay)
        {
            DrawDebugRay(ctx, isVisible);
        }
        
        return RequiredResult == ResultType.Clear ? isVisible : !isVisible;
    }
    
    private bool CheckForward(EnemyContext ctx)
    {
        Vector2 origin = ctx.Self.position;
        Vector2 direction = ctx.Self.right;
        float distance = MaxDistance > 0 ? MaxDistance : 10f;
        
        Vector2 offsetOrigin = origin + direction * OriginOffset;
        float adjustedDistance = distance - OriginOffset;
        
        if (adjustedDistance <= 0)
            return true;
        
        RaycastHit2D hit = Physics2D.Raycast(offsetOrigin, direction, adjustedDistance, ObstacleLayer);
        return hit.collider == null;
    }
    
    private void DrawDebugRay(EnemyContext ctx, bool isVisible)
    {
        Vector2 origin = ctx.Self.position;
        Vector2 direction;
        float distance;
        
        if (Target == RaycastTarget.Player && ctx.Target != null)
        {
            Vector2 targetPos = ctx.Target.position;
            direction = (targetPos - origin).normalized;
            distance = Vector2.Distance(origin, targetPos);
            if (MaxDistance > 0)
                distance = Mathf.Min(MaxDistance, distance);
        }
        else
        {
            direction = ctx.Self.right;
            distance = MaxDistance > 0 ? MaxDistance : 10f;
        }
        
        Vector2 offsetOrigin = origin + direction * OriginOffset;
        float adjustedDistance = distance - OriginOffset;
        
        Color rayColor = isVisible ? Color.green : Color.red;
        Debug.DrawRay(offsetOrigin, direction * adjustedDistance, rayColor, 0.1f);
    }
}