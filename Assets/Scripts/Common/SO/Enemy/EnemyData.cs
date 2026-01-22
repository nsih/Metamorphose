using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "SO/Enemy/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    [Header("Basic Stats")]
    public int MaxHp = 5;
    public float MoveSpeed = 5f;
    public int Damage = 1;
    public float AttackCoolTime = 1;

    [Header("Range Settings")]
    public float AggroRange = 10;
    public float AttackRange = 7.5f;

    [Header("Normal Phase Strategies")]
    public EnemyMoveStrategySO MoveStrategy; 
    public EnemyAttackStrategySO AttackStrategy; 

    [Header("Enraged Phase (Optional)")]
    public bool HasEnragedPhase;
    [Range(0f, 1f)]
    [Tooltip("HP 비율 (0.3 = 30% 이하일 때 발악)")]
    public float EnrageThreshold = 0.3f;
    public EnemyMoveStrategySO EnragedMoveStrategy;
    public EnemyAttackStrategySO EnragedAttackStrategy;
    public float EnragedMoveSpeed = 7f;
    public float EnragedAttackCoolTime = 0.5f;

}