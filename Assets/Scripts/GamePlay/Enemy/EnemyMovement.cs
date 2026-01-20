using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private Transform _target;
    private EnemyDataSO _data;

    public void Initialize(Transform target, EnemyDataSO data)
    {
        _target = target;
        _data = data;
    }

    public void ExecuteMove(EnemyMoveStrategySO strategy)
    {
        if (_target == null || _data == null || strategy == null) return;
        
        strategy.Move(transform, _target, _data);
    }
}