using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int amount, UnityEngine.Vector3 hitPoint);
}