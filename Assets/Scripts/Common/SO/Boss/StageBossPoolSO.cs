using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageBossPool_", menuName = "SO/Boss/Stage Boss Pool")]
public class StageBossPoolSO : ScriptableObject
{
    [SerializeField] private List<GameObject> _bossPrefabs;

    // 랜덤 보스 프리팹 반환, 없으면 null
    public GameObject GetRandomBoss()
    {
        if (_bossPrefabs == null || _bossPrefabs.Count == 0)
        {
            Debug.LogWarning("StageBossPoolSO: 등록된 보스 없음");
            return null;
        }

        int index = Random.Range(0, _bossPrefabs.Count);
        return _bossPrefabs[index];
    }
}