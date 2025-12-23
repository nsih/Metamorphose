using UnityEngine;

namespace Common 
{
    // 방의 상태 (대기 -> 전투 -> 완료)
    public enum RoomState
    {
        Idle,
        Battle,
        Complete
    }

    public enum EnemyType
    {
        asd
    }

    public enum EnemyState
    {
        Idle,   // 대기/배회
        Chase,  // 추적
        Attack  // 공격
    }
}