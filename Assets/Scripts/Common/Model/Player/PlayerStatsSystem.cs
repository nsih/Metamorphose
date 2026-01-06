public class PlayerStatsSystem
{
    public float MoveSpeed { get; private set; }
    public int RewardChoiceCount { get; private set; }
    
    private PlayerStat _stat;
    public float TimeSlowFactor => _stat.TimeSlowFactor;
    public float SlowMotionDuration => _stat.SlowMotionDuration;

    public PlayerStatsSystem(PlayerStat stat)
    {
        _stat = stat;
        MoveSpeed = stat.MoveSpeed;
        RewardChoiceCount = stat.RewardChoiceCount;
    }

    public void IncreaseMoveSpeed(float amount)
    {
        MoveSpeed += amount;
    }

    public void IncreaseRewardChoiceCount(int amount)
    {
        RewardChoiceCount += amount;
    }
}