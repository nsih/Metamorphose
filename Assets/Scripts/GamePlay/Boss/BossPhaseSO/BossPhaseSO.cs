using UnityEngine;

[CreateAssetMenu(fileName = "BossPhase_", menuName = "SO/Boss/Phase")]
public class BossPhaseSO : ScriptableObject
{
    [Header("기본")]
    public string PhaseName;

    [Header("AI")]
    public EnemyBrainSO Brain;

    [Header("전환 조건")]
    [Range(0f, 1f)]
    public float HPThresholdToExit;

    [Header("진입 연출")]
    public float IntroDelay;
    public bool IsInvulnerableDuringIntro;
}