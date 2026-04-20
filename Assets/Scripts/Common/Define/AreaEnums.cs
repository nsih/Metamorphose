// Assets/Scripts/Common/Define/AreaEnums.cs

// 장판 형태
public enum AreaShape
{
    Circle,
    Rect
}

// 장판 위치 결정 전략
public enum AreaPositionStrategy
{
    TargetPosition,        // 플레이어 현재 위치
    OwnerPosition,         // 시전자 현재 위치
    OwnerRelativeFixed,    // 시전자 기준 고정 오프셋
    OwnerRelativeRandom,   // 시전자 기준 범위 내 랜덤
    WorldFixed,            // 월드 절대좌표
    WorldRandom            // 방 범위 내 완전 랜덤
}

// 장판 방향 결정 전략
public enum AreaDirection
{
    None,           // 회전 없음
    TowardTarget,   // 플레이어 방향 조준
    FixedAngle,     // 고정 각도
    OwnerForward    // 시전자가 바라보는 방향
}