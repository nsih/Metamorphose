using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class SpawnEntry //웨이브 마다 리스트로 만들거임 (리스트 길이는 해당 방의 스폰 위치개수)
{
    public GameObject EnemyPrefab; // 최적화하려면 다른 방법이 필요할듯
    public int SpawnPointIndex;    // room data에 붙어 있을 예정
    public int Count;              // 스폰할 적 수
}

// n phase wave (SO 마다 리스트로 만들거임)
[System.Serializable]
public class Wave
{
    public List<SpawnEntry> SpawnGroups;
}

[CreateAssetMenu(fileName = "RoomWaveData", menuName = "SO/RoomWaveData")]
public class RoomWaveData : ScriptableObject
{
    public List<Wave> Waves; // 전체 시나리오
}