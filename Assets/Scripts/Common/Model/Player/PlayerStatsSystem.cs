using Common;
using Common.Model;

public class PlayerStatsSystem
{
    private readonly PlayerStat _stat;

    public ModifiableStat MoveSpeed { get; private set; }
    public ModifiableStat RewardChoiceCount { get; private set; }

    public float TimeSlowFactor => _stat.TimeSlowFactor;
    public float SlowMotionDuration => _stat.SlowMotionDuration;

    public PlayerStatsSystem(PlayerStat stat)
    {
        _stat = stat;
        MoveSpeed = new ModifiableStat(stat.MoveSpeed);
        RewardChoiceCount = new ModifiableStat(stat.RewardChoiceCount);
    }
}