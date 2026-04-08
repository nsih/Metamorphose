// Assets/Scripts/GamePlay/Boss/BossProfileSO/BossProfileSO.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Boss_", menuName = "SO/Boss/Profile")]
public class BossProfileSO : ScriptableObject
{
    [Header("기본 정보")]
    public string BossName;
    public string BossNameKr;
    public int TotalMaxHP;

    [Header("페이즈")]
    public List<BossPhaseSO> Phases;

    [Header("사망 연출")]
    public DeathEffectSO DeathEffect;
    public float DeathDelay;

    [Header("UI")]
    public Sprite BossIcon;

    [Header("인트로 연출")]
    [Tooltip("null이면 인트로 스킵")]
    public Sprite CutinSprite;
    public CutinDirection CutinDirection = CutinDirection.Right;
    public float CameraPanDuration = 0.6f;
    public float CutinHoldDuration = 1.0f;

    [Tooltip("빈 문자열이면 Yarn 대화 스킵")]
    public string IntroYarnNode;

    public bool HasIntro => CutinSprite != null;

    // HP 비율에 따라 현재 있어야 할 페이즈 인덱스 반환
    public int GetPhaseIndex(float hpRatio)
    {
        if (Phases == null || Phases.Count == 0) return 0;

        for (int i = 0; i < Phases.Count; i++)
        {
            if (hpRatio <= Phases[i].HPThresholdToExit)
            {
                return i + 1;
            }
        }

        return 0;
    }

    // SO 유효성 검증
    public bool Validate()
    {
        bool valid = true;

        if (Phases == null || Phases.Count == 0)
        {
            Debug.LogError($"BossProfileSO [{BossName}]: Phases 비어있음");
            return false;
        }

        BossPhaseSO lastPhase = Phases[Phases.Count - 1];
        if (lastPhase.HPThresholdToExit != 0f)
        {
            Debug.LogWarning($"BossProfileSO [{BossName}]: 마지막 페이즈 HPThresholdToExit이 0이 아님");
            valid = false;
        }

        for (int i = 0; i < Phases.Count; i++)
        {
            if (Phases[i].Brain == null)
            {
                Debug.LogError($"BossProfileSO [{BossName}]: Phase[{i}] Brain null");
                valid = false;
            }
        }

        return valid;
    }
}