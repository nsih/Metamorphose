using UnityEngine;
using BulletPro;


public class EnemyTest : MonoBehaviour
{
    [SerializeField] private BulletReceiver _receiver;

    void Start()
    {
        if (_receiver == null) 
        {
            _receiver = GetComponent<BulletReceiver>();
        }

        _receiver.OnHitByBullet.AddListener(Die);
    }
    public void Die(Bullet bullet, Vector3 hitPoint)
    {
        Debug.Log($"hit by {bullet.gameObject.name}");
        
        Destroy(gameObject);
    }
}
