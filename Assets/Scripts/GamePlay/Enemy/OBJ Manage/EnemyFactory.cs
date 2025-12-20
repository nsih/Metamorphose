using UnityEngine;

public class EnemyFactory
{
    public EnemyFactory()
    {
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


        //이동전략 연결
        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
        if (movement != null)
        {
            // 나중에 Transform을 팩토리 생성자에서 캐싱 or GameManager 등 전역에서 참조로 변경해야함
            Transform target = GameObject.FindWithTag("Player")?.transform;
            
            movement.Initialize(target, data);
        }

        // Pool disable
        enemy.SetReleaseAction(() => EnemyPoolManager.Instance.Release(enemy));

        return enemy;
    }
}