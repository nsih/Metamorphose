using UnityEngine;
using Reflex.Attributes;

public class BulletTimeManager : MonoBehaviour
{
    [Inject] private PlayerStat _playerStat;

    private float _defaultFixedDeltaTime;
    private float _currentTimer = 0f;

    public bool IsBulletTimeActive => _currentTimer > 0;

    void Awake()
    {
        _defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    public void TriggerSlowMotion()
    {
        if (_playerStat == null) return;

        // 타이머 리필
        _currentTimer = _playerStat.SlowMotionDuration;

        // on
        SetTimeScale(_playerStat.TimeSlowFactor);
        Debug.Log("bullet Time On");
    }

    void Update()
    {
        if (_currentTimer > 0)
        {
            _currentTimer -= Time.unscaledDeltaTime;

            if (_currentTimer <= 0)
            {
                _currentTimer = 0f;
                SetTimeScale(1f);
            }
        }
    }

    private void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = _defaultFixedDeltaTime * scale;
    }

    void OnDestroy()
    {
        SetTimeScale(1f); // 안전하게 복구
    }
}