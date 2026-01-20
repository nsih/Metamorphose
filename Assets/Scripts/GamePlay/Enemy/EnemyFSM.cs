using UnityEngine;
using Common;

public class EnemyFSM : MonoBehaviour
{
    [Header("State References")]
    [SerializeField] private EnemyStateSO _idleState;
    [SerializeField] private EnemyStateSO _chaseState;
    [SerializeField] private EnemyStateSO _attackState;

    private EnemyStateSO _currentState;
    private EnemyContext _ctx;
    private bool _isInitialized;

    public EnemyState CurrentStateType => _currentState != null ? _currentState.StateType : EnemyState.Idle;

    public void Initialize(EnemyContext ctx)
    {
        _ctx = ctx;
        _currentState = _idleState;
        _isInitialized = true;

        if (_currentState != null)
            _currentState.Enter(_ctx);
    }

    private void Update()
    {
        if (!_isInitialized || _ctx == null || _ctx.IsDead) return;

        _ctx.CheckEnrage();
        _currentState?.Execute(_ctx);
        EvaluateTransitions();
    }

    private void EvaluateTransitions()
    {
        if (_currentState == null || _ctx.Data == null) return;

        float dist = _ctx.DistanceToTarget;
        EnemyState currentType = _currentState.StateType;

        switch (currentType)
        {
            case EnemyState.Idle:
                if (dist <= _ctx.Data.AggroRange)
                    ChangeState(_chaseState);
                break;

            case EnemyState.Chase:
                if (dist <= _ctx.Data.AttackRange)
                    ChangeState(_attackState);
                else if (dist > _ctx.Data.AggroRange)
                    ChangeState(_idleState);
                break;

            case EnemyState.Attack:
                if (dist > _ctx.Data.AttackRange)
                    ChangeState(_chaseState);
                break;
        }
    }

    private void ChangeState(EnemyStateSO newState)
    {
        if (newState == null || newState == _currentState) return;

        _currentState?.Exit(_ctx);
        _currentState = newState;
        _currentState.Enter(_ctx);
    }

    public void Reset()
    {
        _isInitialized = false;
        
        if (_currentState != null && _ctx != null)
        {
            _currentState.Exit(_ctx);
            _currentState.Reset(_ctx);
        }
        
        _currentState = _idleState;
    }

    public void Stop()
    {
        _isInitialized = false;
        _currentState?.Exit(_ctx);
    }
}