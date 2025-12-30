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

    #region 'stat'
    public enum StatModType
    {
        Flat = 100,        // 깡스탯 (공격력 +10)
        PercentAdd = 200,  // 합연산 (공격력 +10%, +20% -> 총 30%)
        PercentMult = 300  // 곱연산 (최종 데미지 2배)
    }
    #endregion
}