using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using BulletPro;

public class PlayerModel : IDisposable
{
    // 유사 파사드
    private PlayerStat _stat;
    private CancellationTokenSource _cts; // 비동기 작업 취소용

    
    //초기화는 SO로 초기화 되는 데이터
    public float MaxHP { get; private set; }
    public float MoveSpeed { get; private set; }
    public float JumpForce { get; private set; }
    public float DashSpeed { get; private set; }
    public float DashDuration { get; private set; }


    //대쉬관련은 조정하면 벨붕될것 같아서 읽기만 하게 놔둠
    public int MaxDashChargeStack => _stat.MaxDashChargeStack;
    public float DashChargeTime => _stat.DashChargeTime;
    public float TimeSlowFactor => _stat.TimeSlowFactor;
    public float SlowMotionDuration => _stat.SlowMotionDuration;


    // Runtime Data
    public AsyncReactiveProperty<float> CurrentHP { get; private set; }
    public AsyncReactiveProperty<int> CurrentDashCount { get; private set; }
    public AsyncReactiveProperty<float> DashCooldownNormalized { get; private set; }


    // 공격관련 능력치
    public EmitterProfile CurrentProfile { get; private set; }
    public float Damage { get; private set; }
    public float FireRate { get; private set; }
    public float Range { get; private set; }
    // 무기 관련 파라메터
    public int ProjectileCount { get; private set; }
    public float SpreadAngle { get; private set; }
    public float SpeedScale { get; private set; }
    public float HomingStrength { get; private set; }


    public PlayerModel(PlayerStat stat, PlayerWeaponData initialWeapon)
    {
        _stat = stat;
        _cts = new CancellationTokenSource();


        //SO로 초기화
        MaxHP = _stat.MaxHealth;
        MoveSpeed = _stat.MoveSpeed;
        JumpForce = _stat.JumpForce;
        DashSpeed = _stat.DashSpeed;
        DashDuration = _stat.DashDuration;

        // SO 에 따른 무기 스탯 초기화 
        if (initialWeapon != null)
        {
            SetWeapon(initialWeapon);
        }
        else
        {
            Debug.Log("Weapon Data Connection error");
            Damage = 1f;
            ProjectileCount = 1;
            FireRate = 0.5f;
            SpeedScale = 1f;
        }




        // 반응형 프로퍼티 초기화
        CurrentHP = new AsyncReactiveProperty<float>(_stat.MaxHealth);
        CurrentDashCount = new AsyncReactiveProperty<int>(_stat.MaxDashChargeStack);
        DashCooldownNormalized = new AsyncReactiveProperty<float>(0f);

        StartCooldownLoop(_cts.Token).Forget();
    }



    // 무기 교체
    public void SetWeapon(PlayerWeaponData data)
    {
        CurrentProfile = data.Profile;
        
        Damage = data.BaseDamage;
        FireRate = data.BaseFireRate;
        Range = data.BaseRange;
        
        ProjectileCount = data.BaseProjectileCount;
        SpreadAngle = data.BaseSpreadAngle;
        SpeedScale = data.BaseSpeedScale;
        HomingStrength = data.BaseHomingStrength;

        Debug.Log(data.name);
    }


    //HP
    public void TakeDamage(float amount)
    {
        CurrentHP.Value -= amount;
        if (CurrentHP.Value < 0) CurrentHP.Value = 0;
    }
    public void Heal(float amount)
    {
        float nextHp = CurrentHP.Value + amount;
        if (nextHp > MaxHP) nextHp = MaxHP;
        CurrentHP.Value = nextHp;
    }



    public bool TryConsumeDash()
    {
        if (CurrentDashCount.Value > 0)
        {
            CurrentDashCount.Value--;

            return true;
        }
        return false;
    }


    #region '보상 관련 메소드'
    public void IncreaseMaxHP(float amount)
    {
        MaxHP += amount;
        CurrentHP.Value += amount; 
    }

    public void IncreaseDamage(float amount)
    {
        Damage += amount;
    }

    public void IncreaseMultishot(int amount)
    {
        ProjectileCount += amount;
        SpreadAngle += (amount * 5f); //대충 탄퍼짐 늘리기
    }

    public void IncreaseAttackSpeed(float percent)
    {
        // 10% 증가 -> 쿨타임 0.9배 감소
        FireRate *= (1.0f - percent);
        if (FireRate < 0.05f) FireRate = 0.05f; // 최소 제한
    }

    #endregion


    public void Dispose()
    {
        _cts?.Cancel(); // 루프 정지
        _cts?.Dispose();
        CurrentHP?.Dispose();
        CurrentDashCount?.Dispose();
        DashCooldownNormalized?.Dispose();
    }

    private async UniTaskVoid StartCooldownLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.WaitUntil(() => CurrentDashCount.Value < MaxDashChargeStack, cancellationToken: token);

            float localTimer = 0f; 

            while (localTimer < DashChargeTime)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);

                // 불렛타임 무시 (Unscaled Time)
                localTimer += Time.unscaledDeltaTime;

                DashCooldownNormalized.Value = localTimer / DashChargeTime;
            }

            CurrentDashCount.Value++;
            DashCooldownNormalized.Value = 0f;
        }
    }
}