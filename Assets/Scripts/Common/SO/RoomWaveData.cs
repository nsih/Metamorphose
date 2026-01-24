using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SpawnEntry
{
    public EnemyBrainSO EnemyBrain;
    public int SpawnPointIndex;
    public int Count;
}

[System.Serializable]
public class Wave
{
    public float PostWaveDelay = 1.0f;
    public List<SpawnEntry> SpawnGroups;
}

[CreateAssetMenu(fileName = "RoomWaveData", menuName = "SO/RoomWaveData")]
public class RoomWaveData : ScriptableObject
{
    public List<Wave> Waves;
}