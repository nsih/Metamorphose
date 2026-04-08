// Assets/Scripts/Common/Interface/ICutinService.cs
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

// 컷인 연출 서비스. 슬라이드 인 - 유지 - 슬라이드 아웃 전체 사이클 실행
public interface ICutinService
{
    UniTask ShowAsync(Sprite sprite, CutinDirection direction, CutinParams param, CancellationToken ct);
}