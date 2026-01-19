using UnityEngine;
using Common;

public abstract class EnemyStateSO : ScriptableObject
{
    public abstract EnemyState StateType { get; }
    
    // 상태 진입 시 호출
    public virtual void Enter(EnemyContext ctx) { }
    
    // 매 프레임 호출, 행동 실행만 담당
    public abstract void Execute(EnemyContext ctx);
    
    // 상태 이탈 시 호출
    public virtual void Exit(EnemyContext ctx) { }
    
    // 풀링 시 초기화용
    public virtual void Reset(EnemyContext ctx) { }
}