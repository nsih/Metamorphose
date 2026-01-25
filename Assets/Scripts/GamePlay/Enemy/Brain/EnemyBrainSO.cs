// Assets/Scripts/GamePlay/Enemy/Brain/EnemyBrainSO.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Brain_", menuName = "SO/Enemy/Brain")]
public class EnemyBrainSO : ScriptableObject
{
    [Header("States")]
    public EnemyStateSO DefaultState;
    
    [Header("Transitions")]
    public List<TransitionRuleSO> Transitions;
    
    [Header("Base Stats")]
    public int MaxHP = 5;
    public float MoveSpeed = 5f;
    public float AttackCoolTime = 1f;
    
    [Header("Enrage (Optional)")]
    public bool HasEnragedPhase;
    [Range(0f, 1f)]
    public float EnrageThreshold = 0.3f;
    public float EnragedMoveSpeed = 7f;
    public float EnragedAttackCoolTime = 0.5f;
    
    [Header("Death")]
    public DeathEffectSO DeathEffect;
    
    [Tooltip("죽음 연출 후 사라지기까지 딜레이")]
    public float DeathDelay = 0f;
    
    public EnemyStateSO EvaluateTransitions(EnemyStateSO currentState, EnemyContext ctx)
    {
        if (Transitions == null) return null;
        
        foreach (var transition in Transitions)
        {
            if (transition == null) continue;
            if (transition.FromState != currentState) continue;
            
            if (transition.Evaluate(ctx))
                return transition.ToState;
        }
        
        return null;
    }
}