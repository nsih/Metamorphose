using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerStat", menuName = "SO/Player/Player Stats")]
public class PlayerStat : ScriptableObject
{
    [Header("Health")]
    public float MaxHealth = 10f;

    [Header("Movement")]
    public float MoveSpeed = 25f;

    [Header("Dash")]
    public float DashSpeed = 50f;
    public float DashDuration = 0.2f;
    public int MaxDashChargeStack = 3;
    public float DashChargeTime = 2f;
    public float PostDashInvincibleDuration = 0.1f;

    [Header("Bullet Time")]
    [Tooltip("시간이 느려지는 배율 (0.1 = 10%의 속도)")]
    [Range(0.01f, 1f)]
    public float TimeSlowFactor = 0.5f;

    [Tooltip("그레이즈 성공 시 불렛 타임 지속 시간 (초)")]
    public float SlowMotionDuration = 5.0f;

    [Header("Reward")]
    [Tooltip("보상 선택지 개수")]
    public int RewardChoiceCount = 3;
}