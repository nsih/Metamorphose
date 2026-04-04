using UnityEngine;
using BulletPro;

public class BulletWallChecker : MonoBehaviour
{
    [SerializeField] private LayerMask _wallLayer;

    void LateUpdate()
    {
        if (BulletPoolManager.instance == null) return;

        Bullet[] pool = BulletPoolManager.instance.pool;
        if (pool == null) return;

        for (int i = 0; i < pool.Length; i++)
        {
            Bullet b = pool[i];
            if (b == null || b.isAvailableInPool) continue;

            Vector2 pos = b.self.position;
            Vector2 dir = b.self.up;
            float speed = b.moduleMovement.baseSpeed;
            float rayLen = Mathf.Max(speed * Time.deltaTime, 0.5f);

            RaycastHit2D hit = Physics2D.Raycast(pos, dir, rayLen, _wallLayer);

            if (hit.collider != null)
            {
                b.Die();
            }
        }
    }
}