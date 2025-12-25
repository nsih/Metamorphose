using UnityEngine;

namespace Common 
{

    #region 'Room'
    public enum RoomType
    {
        Start,
        Battle,
        Shop,
        Elite,
        Boss,
        Event
    }
    // 방의 상태 (대기 -> 전투 -> 완료)
    public enum RoomState
    {
        Idle,
        Battle,
        Complete
    }
    #endregion

    #region 'enemy'
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
    #endregion
}