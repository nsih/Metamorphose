using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class PlayerModel
{
    // 유사 파사드
    private PlayerStat _stat;

    
    //초기화는 SO로 초기화 되는 데이터
    public float MaxHP => _stat.MaxHealth;
    public float MoveSpeed => _stat.MoveSpeed;
    public float JumpForce => _stat.JumpForce;
    public float DashSpeed => _stat.DashSpeed;
    public float DashDuration => _stat.DashDuration;
    public int MaxDashChargeStack => _stat.MaxDashChargeStack;
    public float DashChargeTime => _stat.DashChargeTime;
    public float TimeSlowFactor => _stat.TimeSlowFactor;
    public float SlowMotionDuration => _stat.SlowMotionDuration;



    // runtime data
    public AsyncReactiveProperty<float> CurrentHP { get; private set; } //HP
    public AsyncReactiveProperty<int> CurrentDashCount { get; private set; }    //대쉬 스택
    public AsyncReactiveProperty<float> DashCooldownNormalized { get; private set; } //대쉬 쿨다운




    //
    private float _currentCooldownTimer;


    // 생성자 (SO로 초기화 되는 데이터로 초기화되는 런타임 데이터)
    // 인스톨러에서 stat 주입
    public PlayerModel(PlayerStat stat)
    {
        _stat = stat;

        CurrentHP = new AsyncReactiveProperty<float>(_stat.MaxHealth);
        CurrentDashCount = new AsyncReactiveProperty<int>(_stat.MaxDashChargeStack);
        DashCooldownNormalized = new AsyncReactiveProperty<float>(0f);
    }



    // 함수
    public void TakeDamage(float amount)
    {
        CurrentHP.Value -= amount;

        if (CurrentHP.Value < 0) CurrentHP.Value = 0;
    }

    public bool TryConsumeDash()
    {
        if (CurrentDashCount.Value > 0)
        {
            CurrentDashCount.Value--; // 자동 통지
            return true;
        }
        return false;
    }

    public void UpdateDashCooldown(float deltaTime)
    {
        // 대시가 꽉 찼으면 쿨타임 계산 안 함
        if (CurrentDashCount.Value >= MaxDashChargeStack) 
        {
            _currentCooldownTimer = 0f;
            if (DashCooldownNormalized.Value != 0f) DashCooldownNormalized.Value = 0f;
            return;
        }

        // 시간 누적
        _currentCooldownTimer += deltaTime;

        // UI 갱신을 위해 0.0 ~ 1.0 사이 값으로 변환하여 알림
        DashCooldownNormalized.Value = _currentCooldownTimer / DashChargeTime;

        // 충전 완료 체크
        if (_currentCooldownTimer >= DashChargeTime)
        {
            CurrentDashCount.Value++; // 대시 충전!
            _currentCooldownTimer = 0f;
            DashCooldownNormalized.Value = 0f; // 게이지 리셋
        }
    }


    public void Dispose()
    {
        CurrentHP?.Dispose();
        CurrentDashCount?.Dispose();
        DashCooldownNormalized?.Dispose();
    }
}