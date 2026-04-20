using R3;
using UnityEngine;
using BulletPro;

public class BossContext
{
    public BossProfileSO Profile { get; private set; }
    public ReactiveProperty<int> CurrentHP { get; private set; }
    public ReactiveProperty<int> CurrentPhaseIndex { get; private set; }
    public bool IsInvulnerable { get; set; }
    public bool IsDead { get; set; }
    public EnemyContext InnerContext { get; private set; }

    public float HPRatio => (float)CurrentHP.Value / Profile.TotalMaxHP;
    public BossPhaseSO CurrentPhase => Profile.Phases[CurrentPhaseIndex.Value];

    public BossContext(BossProfileSO profile, Transform self, SpriteRenderer sr, BulletEmitter emitter, Rigidbody2D rb)
    {
        Profile = profile;
        CurrentHP = new ReactiveProperty<int>(profile.TotalMaxHP);
        CurrentPhaseIndex = new ReactiveProperty<int>(0);
        InnerContext = new EnemyContext(self, sr, emitter, rb);
    }

    public void Dispose()
    {
        CurrentHP.Dispose();
        CurrentPhaseIndex.Dispose();
    }
}