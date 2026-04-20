// Assets/Scripts/GamePlay/AreaAttack/AreaAttackEntry.cs
// 2026-04-20 직렬화 클래스 신규
using UnityEngine;

[System.Serializable]
public class AreaAttackEntry
{
    public AreaAttackConfigSO Config;
    [Tooltip("이전 엔트리 대비 발사 딜레이. 0 = 동시")]
    public float Delay;
}
