using UnityEngine;
using BulletPro;
using Unity.VisualScripting.YamlDotNet.Core;


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
}