// Assets/Scripts/GamePlay/Enemy/EnemyContextKeys.cs
// 2026-04-20 AreaAttackActive 키 추가
public static class EnemyContextKeys
{
    public static readonly int ShootCount = "ShootCount".GetHashCode();
    public static readonly int TeleportComplete = "TeleportComplete".GetHashCode();
    public static readonly int StrafeTime = "StrafeTime".GetHashCode();
    public static readonly int IsTeleporting = "IsTeleporting".GetHashCode();
    public static readonly int AreaAttackActive = "AreaAttackActive".GetHashCode();
}