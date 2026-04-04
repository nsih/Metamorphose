// Assets/Scripts/GamePlay/Weapon/TilemapCollisionBehaviour.cs
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

        if (hit.collider != null)
        {
            bullet.Die();
        }
    }

    public override void OnBulletBirth()
    {
        base.OnBulletBirth();
    }

    public override void OnBulletDeath()
    {
        Debug.Log($"TilemapCollision OnBulletDeath called, isDestructing={isDestructing}");
        base.OnBulletDeath();
    }
}