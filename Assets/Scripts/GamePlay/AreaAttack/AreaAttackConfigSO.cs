// Assets/Scripts/GamePlay/AreaAttack/AreaAttackConfigSO.cs
// 2026-04-20 장판 설정 SO 신규
using UnityEngine;

[CreateAssetMenu(fileName = "AreaAtk_", menuName = "SO/Enemy/AreaAttack/Config")]
public class AreaAttackConfigSO : ScriptableObject
{
    [Header("형태")]
    public AreaShape Shape = AreaShape.Circle;
    [Tooltip("Circle: x=반지름(y무시). Rect: x=폭, y=길이")]
    public Vector2 Size = new Vector2(3f, 3f);

    [Header("위치")]
    public AreaPositionStrategy PositionStrategy = AreaPositionStrategy.TargetPosition;
    [Tooltip("OwnerRelativeFixed 전용")]
    public Vector2 FixedOffset;
    [Tooltip("OwnerRelativeRandom 전용")]
    public float RandomRadius = 5f;
    [Tooltip("WorldFixed 전용")]
    public Vector2 WorldPosition;
    [Tooltip("WorldRandom 전용")]
    public Rect RandomBounds = new Rect(-10, -10, 20, 20);

    [Header("방향")]
    public AreaDirection Direction = AreaDirection.None;
    [Tooltip("FixedAngle 전용 (도)")]
    public float FixedAngleDeg;
    [Tooltip("true: 경고 시작 시 방향 고정 / false: 경고 중 추적, 발동 직전 고정")]
    public bool LockDirectionOnWarning = true;

    [Header("타이밍")]
    [Tooltip("경고 단계 지속시간 (최소 1초)")]
    public float WarningDuration = 1.5f;
    [Tooltip("발동 후 장판 유지 시간. 0이면 즉시 소멸")]
    public float LingerDuration = 0f;
    [Tooltip("Linger 중 지속 판정 여부")]
    public bool ContinuousDamage = false;
    [Tooltip("지속 판정 간격 (초). Enter/Stay 이중 방지도 이 값 사용")]
    public float HitInterval = 0.5f;

    [Header("데미지")]
    public float Damage = 1f;

    [Header("비주얼")]
    public Sprite WarningSprite;
    public Sprite ActivateSprite;
    public Color WarningColor = new Color(1f, 0.3f, 0.3f, 0.4f);
    public Color ActivateColor = new Color(1f, 0.1f, 0.1f, 0.9f);
    public float FadeInDuration = 0.3f;
    public float FadeOutDuration = 0.3f;
    public float PulseSpeed = 3f;

    [Header("사운드 (빈 문자열 = 없음)")]
    public string WarningStartSFX = "";
    public string WarningLoopSFX = "";
    public string ActivateSFX = "";

    private void OnValidate()
    {
        WarningDuration = Mathf.Max(1f, WarningDuration);
        HitInterval = Mathf.Max(0.1f, HitInterval);
        Size = new Vector2(Mathf.Max(0.1f, Size.x), Mathf.Max(0.1f, Size.y));
    }
}
