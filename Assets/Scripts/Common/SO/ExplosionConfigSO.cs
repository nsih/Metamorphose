using UnityEngine;

[CreateAssetMenu(fileName = "ExplosionConfig", menuName = "SO/ExplosionConfigSO")]
public class ExplosionConfigSO : ScriptableObject
{
    [Header("Explosion")]
    public float Radius = 3f;
    public float Damage = 10f;
    public float Force = 0f;
    public LayerMask TargetLayer;
    
    [Header("Visual")]
    public Sprite ExplosionSprite;
    public float VisualDuration = 0.3f;
    public float VisualScale = 1f;
    public Color VisualColor = Color.white;
}