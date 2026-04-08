// Assets/Scripts/Common/DTO/CutinParams.cs
using System;

// 컷인 연출 타이밍 파라미터
[Serializable]
public struct CutinParams
{
    public float SlideInDuration;
    public float HoldDuration;
    public float SlideOutDuration;

    public static CutinParams Default => new CutinParams
    {
        SlideInDuration = 0.3f,
        HoldDuration = 1.0f,
        SlideOutDuration = 0.2f
    };
}