using UnityEngine;
using BulletPro;

public class WallInteraction : MonoBehaviour
{
    [SerializeField] private BulletReceiver _receiver;

    void Start()
    {
        if (_receiver == null) 
        {
            _receiver = GetComponent<BulletReceiver>();
        }

        _receiver.OnHitByBullet.AddListener(Block);
    }
    public void Block(Bullet bullet, Vector3 hitPoint)
    {
        Debug.Log("walling");
        bullet.Die();
    }
}