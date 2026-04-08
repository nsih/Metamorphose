// Assets/Scripts/UI/CutinView.cs
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using LitMotion;

public class CutinView : MonoBehaviour, ICutinService
{
    [Header("UI")]
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _imageRect;
    [SerializeField] private Image _cutinImage;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("슬라이드 설정")]
    [SerializeField] private float _offscreenOffset = 1200f;

    private void Awake()
    {
        if (_canvas != null)
        {
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 9000;
        }

        _canvasGroup.alpha = 0f;
        _imageRect.gameObject.SetActive(false);
    }

    public async UniTask ShowAsync(Sprite sprite, CutinDirection direction, CutinParams param, CancellationToken ct)
    {
        if (sprite == null) return;

        _cutinImage.sprite = sprite;
        _cutinImage.SetNativeSize();
        _imageRect.gameObject.SetActive(true);

        // 방향에 따른 시작/종료 위치
        float sign = direction == CutinDirection.Right ? 1f : -1f;
        float startX = sign * _offscreenOffset;
        float endX = 0f;
        float exitX = -sign * _offscreenOffset;

        _imageRect.anchoredPosition = new Vector2(startX, _imageRect.anchoredPosition.y);
        _canvasGroup.alpha = 0f;

        // 슬라이드 인 + 페이드 인
        var slideInHandle = LMotion.Create(startX, endX, param.SlideInDuration)
            .WithEase(Ease.OutCubic)
            .Bind(x =>
            {
                var pos = _imageRect.anchoredPosition;
                pos.x = x;
                _imageRect.anchoredPosition = pos;
            });

        var fadeInHandle = LMotion.Create(0f, 1f, param.SlideInDuration * 0.5f)
            .Bind(a => _canvasGroup.alpha = a);

        await UniTask.WaitUntil(() => !slideInHandle.IsActive(), cancellationToken: ct);

        // 유지
        await UniTask.Delay(
            (int)(param.HoldDuration * 1000),
            cancellationToken: ct);

        // 슬라이드 아웃 + 페이드 아웃
        var slideOutHandle = LMotion.Create(endX, exitX, param.SlideOutDuration)
            .WithEase(Ease.InCubic)
            .Bind(x =>
            {
                var pos = _imageRect.anchoredPosition;
                pos.x = x;
                _imageRect.anchoredPosition = pos;
            });

        var fadeOutHandle = LMotion.Create(1f, 0f, param.SlideOutDuration)
            .Bind(a => _canvasGroup.alpha = a);

        await UniTask.WaitUntil(() => !slideOutHandle.IsActive(), cancellationToken: ct);

        _imageRect.gameObject.SetActive(false);
    }
}