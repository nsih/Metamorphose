using UnityEngine;
using BulletPro;

[CreateAssetMenu(fileName = "BulletProAttack", menuName = "SO/Attack/BulletPro")]
public class BulletProAttackStrategySO : EnemyAttackStrategySO
{
    public EmitterProfile emitterProfile;
    public Vector3 offset;

    public override void Attack(Transform enemy, Transform target)
    {
        BulletEmitter emitter = enemy.GetComponent<BulletEmitter>();
        if (emitter == null) return;

        emitter.emitterProfile = emitterProfile; 

        emitter.Play();
    }
}
