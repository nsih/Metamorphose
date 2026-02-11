using UnityEngine;

public class DestructibleObject : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHp = 1;
    private int currentHp;
    private void Awake()
    {
        currentHp = maxHp;
    }
    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        
        if (currentHp <= 0)
        {
            Destroy();
        }
    }

    private void Destroy()
    {
        // VFX, Sound는 FMOD 연동 시 추가
        Destroy(gameObject);
    }
}