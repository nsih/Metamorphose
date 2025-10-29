using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerStat", menuName = "SO/Player/Player Stats")]
public class PlayerStat : ScriptableObject
{
    [Header("Health")]
    public float MaxHealth = 10f;

    [Header("Movement")]
    public float MoveSpeed = 5f;

    [Header("JumpForce")]
    public float JumpForce = 10f;
}
