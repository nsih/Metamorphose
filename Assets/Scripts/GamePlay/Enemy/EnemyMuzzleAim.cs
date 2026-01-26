using UnityEngine;

public class EnemyMuzzleAim : MonoBehaviour
{
    private Transform _target;
    
    public void SetTarget(Transform target)
    {
        _target = target;
    }
    
    void Update()
    {
        if (_target == null) return;
        
        Vector2 direction = _target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }
}