using UnityEngine;
using BulletPro;

[CreateAssetMenu(fileName = "NewPlayerWeaponData", menuName = "SO/Player/Player Weapon Data")]
public class PlayerWeaponData : ScriptableObject
{
    [Header("Bullet Pro Asset")]
    public EmitterProfile Profile;

    [Header("플레이어 능력치 초기값")]
    public float BaseDamage = 1;
    public float BaseFireRate = 0.5f;
    public float BaseRange = 30f;

    [Header("무기 초기값")]
    public int BaseProjectileCount = 1;
    public float BaseSpreadAngle = 0f;
    public float BaseSpeedScale = 20.0f;
    public float BaseHomingStrength = 0f;

    [Header("연사 가중치")]
    [Tooltip("꾹 누를 때 쿨타임 배율 (1.2 = 120%)")]
    public float SustainedFireMultiplier = 1.2f;
}