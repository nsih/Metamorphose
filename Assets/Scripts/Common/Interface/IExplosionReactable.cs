using UnityEngine;
using Common;

public interface IExplosionReactable
{
    void OnExplosion(ExplosionOwner owner, Vector3 center, float damage, float force);
}