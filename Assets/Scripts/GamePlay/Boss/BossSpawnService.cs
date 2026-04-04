using UnityEngine;
using System.Collections.Generic;

public class BossSpawnService
{
    private readonly StageBossPoolSO _bossPool;
    private readonly List<Transform> _spawnPoints;

    public BossSpawnService(StageBossPoolSO bossPool, List<Transform> spawnPoints)
    {
        _bossPool = bossPool;
        _spawnPoints = spawnPoints;
    }

    // 보스 프리팹 인스턴스 생성, BossController 반환. 실패 시 null
    public BossController Spawn(Transform target)
    {
        if (_bossPool == null)
        {
            Debug.LogError("BossSpawnService: bossPool null");
            return null;
        }

        GameObject prefab = _bossPool.GetRandomBoss();

        if (prefab == null)
        {
            Debug.LogError("BossSpawnService: 보스 프리팹 없음");
            return null;
        }

        Vector3 spawnPos = Vector3.zero;
        if (_spawnPoints != null && _spawnPoints.Count > 0)
        {
            spawnPos = _spawnPoints[0].position;
        }

        GameObject bossObj = Object.Instantiate(prefab, spawnPos, Quaternion.identity);
        BossController controller = bossObj.GetComponent<BossController>();

        if (controller == null)
        {
            Debug.LogError("BossSpawnService: BossController 없음");
            Object.Destroy(bossObj);
            return null;
        }

        if (target != null)
        {
            controller.SetTarget(target);
        }

        Debug.Log("BossSpawnService: 보스 스폰");
        return controller;
    }
}