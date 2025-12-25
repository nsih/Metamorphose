using UnityEngine;
using Common;

public class EnemyMovement : MonoBehaviour
{
    // 팩토리에서 주입받을 데이터들
    private Transform _target;
    private EnemyDataSO _data;

    // 초기화 함수
    public void Initialize(Transform target, EnemyDataSO data)
    {
        _target = target;
        _data = data;
    }

    private void Update()
    {
        if (_data == null || _data.MoveStrategy == null) return;


        _data.MoveStrategy.Move(transform, _target, _data);
    }
}