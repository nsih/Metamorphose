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

    //이동시 필요한 상태
    public enum NodeState
    {
        Locked,      // 잠김
        Available,   // 선택 가능
        Completed    // 완료됨
    }
    #endregion

    #region 'enemy'
    public enum EnemyType
    {
        asd
    }

    public enum EnemyState
    {
        Idle,
        Chase,
        Attack
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

    #region 'reward'
    public enum RewardType
    {
        // Health
        MaxHP,              // 최대 체력
        MaxHPPercent,       // 최대 체력 % 증가 (+10%)
        Heal,               // 즉시 회복
        
        // amage
        Damage,             // 공격력 증가 (+5)
        DamagePercent,      // 공격력 % 증가 (+10%)
        DamageMultiplier,   // 공격력 배율 (×1.5)
        
        // Fire Rate
        AttackSpeed,        // 공격 속도 증가 (쿨타임 -10%)
        
        // Projectile
        Multishot,          // 발사체 개수 증가 (+1)
        //Projectile Speed,
        //Projectile Range,
        //Projectile Sprite Change,
        //Projectile Bullet Size,
        
        // Movement
        MoveSpeed,          // 이동 속도 증가
        
        // Special
        // DashCharge,      // 대시 충전 개수
        // Homing
        //Piercing,          // 관통 +1
        //Bounce,            // 튕김 +1
        //ChainLightning,    // 스태틱의 단검

        RewardChoiceCount
    }
    
    public enum RewardRarity
    {
        Common,
        Rare,
        Epic,
    }
    #endregion

    public enum ExplosionOwner
    {
        Player,
        Enemy,
        Environment
    }
}