using UnityEngine;

public class EnemyFactory
{
    Transform _playerTransform;

    public EnemyFactory(Transform playerTransform)
    {
        _playerTransform = playerTransform;
    }

    public Enemy Create(Vector3 position, EnemyDataSO data)
    {
        // get
        Enemy enemy = EnemyPoolManager.Instance.Get();

        // location
        enemy.transform.position = position;
        enemy.transform.rotation = Quaternion.identity;

        // SO 주입
        enemy.Initialize(data);

        //플레이어 위치
        enemy.SetTarget(_playerTransform);


        //이동전략 연결
        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
        if (movement != null)
        {
            // 나중에 Transform을 팩토리 생성자에서 캐싱 or GameManager 등 전역에서 참조로 변경해야함
            // 25.12.21 transform 받아오는 걸로 수정 -> 태그 쓸 필요 없다.
            movement.Initialize(_playerTransform, data);
        }

        // Pool disable
        enemy.SetReleaseAction(() => EnemyPoolManager.Instance.Release(enemy));

        return enemy;
    }
}