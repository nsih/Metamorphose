public class PlayerStatsSystem
{
    public float MoveSpeed { get; private set; }
    public float JumpForce { get; private set; }
    
    private PlayerStat _stat;
    public float TimeSlowFactor => _stat.TimeSlowFactor;
    public float SlowMotionDuration => _stat.SlowMotionDuration;

    public PlayerStatsSystem(PlayerStat stat)
    {
        _stat = stat;
        MoveSpeed = stat.MoveSpeed;
        JumpForce = stat.JumpForce;
    }

    public void IncreaseMoveSpeed(float amount)
    {
        MoveSpeed += amount;
    }
}