using UnityEngine;

public class EnemyFSM : MonoBehaviour
{
    private EnemyBrainSO _brain;
    private EnemyStateSO _currentState;
    private EnemyContext _ctx;
    private bool _isInitialized;

    public EnemyStateSO CurrentState => _currentState;

    public void Initialize(EnemyBrainSO brain, EnemyContext ctx)
    {
        _brain = brain;
        _ctx = ctx;
        
        if (_brain == null || _brain.DefaultState == null)
        {
            Debug.LogError("EnemyFSM: brain or default state null");
            return;
        }
        
        _currentState = _brain.DefaultState;
        _currentState.Enter(_ctx);
        _isInitialized = true;
    }

    private void Update()
    {
        if (!_isInitialized) return;
        if (_ctx == null || _ctx.IsDead) return;
        
        // 상태 진입 시간 추적
        _ctx.TimeInCurrentState += Time.deltaTime;
        
        _ctx.CheckEnrage();
        _currentState?.Execute(_ctx);
        EvaluateTransitions();
    }

    private void EvaluateTransitions()
    {
        if (_brain == null || _currentState == null) return;
        
        EnemyStateSO nextState = _brain.EvaluateTransitions(_currentState, _ctx);
        
        if (nextState != null && nextState != _currentState)
        {
            ChangeState(nextState);
        }
    }

    private void ChangeState(EnemyStateSO newState)
    {
        _currentState?.Exit(_ctx);
        _currentState = newState;
        _ctx.TimeInCurrentState = 0f;
        _currentState?.Enter(_ctx);
    }

    public void Stop()
    {
        _isInitialized = false;
        _currentState?.Exit(_ctx);
    }

    public void Reset()
    {
        _isInitialized = false;
        
        if (_currentState != null && _ctx != null)
            _currentState.Exit(_ctx);
        
        _currentState = _brain?.DefaultState;
    }
}