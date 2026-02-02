using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class StateTransition
{
    public EnemyStateSO FromState;
    public EnemyStateSO ToState;
    
    [Tooltip("모든 조건이 만족해야 전환")]
    public List<TransitionConditionSO> Conditions;
    
    public bool Evaluate(EnemyContext ctx)
    {
        if (Conditions == null || Conditions.Count == 0)
            return false;
        
        return Conditions.All(c => c != null && c.Evaluate(ctx));
    }
}

[CreateAssetMenu(fileName = "Brain_", menuName = "SO/Enemy/Brain")]
public class EnemyBrainSO : ScriptableObject
{
    [Header("Visual")]
    public Sprite Sprite;
    public Color Color = Color.white;
    public Vector2 Scale = Vector2.one;
    
    [Header("States")]
    public EnemyStateSO DefaultState;
    
    [Header("Global Transitions (Any State)")]
    [Tooltip("FromState 무시, 어떤 상태에서든 조건 만족시 전환")]
    public List<StateTransition> GlobalTransitions;
    
    [Header("State Transitions")]
    public List<StateTransition> Transitions;
    
    [Header("Base Stats")]
    public int MaxHP = 5;
    public float MoveSpeed = 5f;
    public float AttackCoolTime = 1f;
    
    [Header("AI Settings")]
    [Tooltip("시야 체크 간격. 높을수록 반응 둔함")]
    [Range(0.1f, 1f)]
    public float VisibilityCheckInterval = 0.1f;
    
    [Header("Enrage (Optional)")]
    public bool HasEnragedPhase;
    [Range(0f, 1f)]
    public float EnrageThreshold = 0.3f;
    public float EnragedMoveSpeed = 7f;
    public float EnragedAttackCoolTime = 0.5f;
    
    [Header("Death")]
    public DeathEffectSO DeathEffect;
    public float DeathDelay = 0f;
    
    public EnemyStateSO EvaluateTransitions(EnemyStateSO currentState, EnemyContext ctx)
    {
        if (GlobalTransitions != null)
        {
            foreach (var trans in GlobalTransitions)
            {
                if (trans.Evaluate(ctx))
                    return trans.ToState;
            }
        }
        
        if (Transitions == null) return null;
        
        foreach (var trans in Transitions)
        {
            if (trans.FromState != currentState) continue;
            
            if (trans.Evaluate(ctx))
                return trans.ToState;
        }
        
        return null;
    }
}