using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

[CreateAssetMenu(fileName = "Utility_TeleportNearPlayer", menuName = "SO/Enemy/Behaviors/Utility/TeleportNearPlayer")]
public class TeleportNearPlayerSO : EnemyUtilityBehaviorSO
{
    [Header("Teleport Settings")]
    public float MinAppearDistance = 2f;
    public float MaxAppearDistance = 5f;
    public float DisappearDuration = 0.5f;
    
    [Header("Wall Check")]
    public LayerMask WallLayer;
    public float CheckRadius = 0.5f;
    public int MaxAttempts = 10;
    
    [Header("Visual")]
    public float DisappearAlpha = 0f;
    
    public override void Execute(EnemyContext ctx)
    {
        if (ctx.IsDead) return;
        if (ctx.Target == null) return;
        
        bool isTeleporting = ctx.GetInt(EnemyContextKeys.IsTeleporting) == 1;
        if (isTeleporting) return;
        
        int complete = ctx.GetInt(EnemyContextKeys.TeleportComplete);
        if (complete == 1) return;
        
        ctx.SetInt(EnemyContextKeys.IsTeleporting, 1);
        TeleportAsync(ctx).Forget();
    }
    
    private async UniTaskVoid TeleportAsync(EnemyContext ctx)
    {
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            ctx.Self.GetComponent<MonoBehaviour>().destroyCancellationToken
        );
        var token = linkedCts.Token;
        
        try
        {
            if (ctx.SpriteRenderer != null)
            {
                Color c = ctx.SpriteRenderer.color;
                c.a = DisappearAlpha;
                ctx.SpriteRenderer.color = c;
            }
            
            if (ctx.Rigidbody != null)
                ctx.Rigidbody.linearVelocity = Vector2.zero;
            
            await UniTask.Delay(
                System.TimeSpan.FromSeconds(DisappearDuration),
                cancellationToken: token
            );
            
            if (ctx.IsDead) return;
            
            Vector3? validPos = FindValidPosition(ctx.Target.position);
            
            if (validPos.HasValue)
            {
                ctx.Self.position = validPos.Value;
            }
            
            if (ctx.SpriteRenderer != null)
            {
                Color c = ctx.SpriteRenderer.color;
                c.a = 1f;
                ctx.SpriteRenderer.color = c;
            }
            
            ctx.SetInt(EnemyContextKeys.TeleportComplete, 1);
        }
        finally
        {
            ctx.SetInt(EnemyContextKeys.IsTeleporting, 0);
            linkedCts.Dispose();
        }
    }
    
    private Vector3? FindValidPosition(Vector3 targetPos)
    {
        for (int i = 0; i < MaxAttempts; i++)
        {
            float distance = Random.Range(MinAppearDistance, MaxAppearDistance);
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            Vector3 candidate = targetPos + new Vector3(randomDir.x, randomDir.y, 0) * distance;
            
            Collider2D hit = Physics2D.OverlapCircle(candidate, CheckRadius, WallLayer);
            
            if (hit == null)
            {
                return candidate;
            }
        }
        
        Debug.LogWarning("TeleportNearPlayer: valid position not found");
        return null;
    }
}