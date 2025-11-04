using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerStat", menuName = "SO/Player/Player Stats")]
public class PlayerStat : ScriptableObject
{
    [Header("Health")]
    public float MaxHealth = 10f;

    [Header("Movement")]
    public float MoveSpeed = 40f;

    [Header("JumpForce")]
    public float JumpForce = 10f;

    [Header("Dash")]
    public float DashSpeed = 80f;
    public float DashDuration = 0.15f;
    public int MaxDashChargeStack = 3;       // 최대 충전 횟수
    public float DashChargeTime = 2f;    // 대쉬 충전 쿨타임
}
