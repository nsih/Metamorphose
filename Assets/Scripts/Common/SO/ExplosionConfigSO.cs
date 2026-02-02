using UnityEngine;

[CreateAssetMenu(fileName = "Explosion_", menuName = "SO/Explosion/Config")]
public class ExplosionConfigSO : ScriptableObject
{
    [Header("Range")]
    public float Radius = 3f;
    
    [Header("Damage")]
    public float Damage = 5f;
    public float Force = 0f;
    
    [Header("Targeting")]
    public LayerMask TargetLayer;
    
    [Header("VFX")]
    public GameObject VFXPrefab;
}