using UnityEngine;
using BulletPro;

public class TilemapCollisionBehaviour : BaseBulletBehaviour
{
    public LayerMask wallLayer;
    
    public override void Update()
    {
        base.Update();
        
        if (bullet == null || isDestructing) return;
        
        Vector2 position = bullet.self.position;
        Vector2 direction = bullet.self.up;
        float speed = bullet.moduleMovement.baseSpeed;
        
        float rayLength = Mathf.Max(speed * Time.deltaTime, 0.5f);
        
        RaycastHit2D hit = Physics2D.Raycast(
            position, 
            direction, 
            rayLength,
            wallLayer
        );
        
        Debug.DrawRay(position, direction * rayLength, hit.collider != null ? Color.red : Color.green);
        
        if (hit.collider != null)
        {
            Debug.Log($"Hit: {hit.collider.name}");
            bullet.Die();
        }
    }
    
    public override void OnBulletBirth()
    {
        base.OnBulletBirth();
        Debug.Log($"Bullet born, wallLayer value: {wallLayer.value}");
    }
}