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
    [SerializeField] private float _yOffset = 80f;
    [SerializeField] private float _maxWidth = 600f;
    [SerializeField] private float _maxHeight = 400f;

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
        FitImageSize(sprite);
        _imageRect.gameObject.SetActive(true);

        // 방향에 따른 시작 위치
        float sign = direction == CutinDirection.Right ? 1f : -1f;
        float startX = sign * _offscreenOffset;
        float endX = 0f;

        _imageRect.anchoredPosition = new Vector2(startX, _yOffset);
        _canvasGroup.alpha = 0f;

        // 슬라이드 인 + 페이드 인
        var slideInHandle = LMotion.Create(startX, endX, param.SlideInDuration)
            .WithEase(Ease.OutCubic)
            .Bind(x =>
            {
                _imageRect.anchoredPosition = new Vector2(x, _yOffset);
            });

        var fadeInHandle = LMotion.Create(0f, 1f, param.SlideInDuration * 0.5f)
            .Bind(a => _canvasGroup.alpha = a);

        await UniTask.WaitUntil(() => !slideInHandle.IsActive(), cancellationToken: ct);

        // 유지
        await UniTask.Delay(
            (int)(param.HoldDuration * 1000),
            cancellationToken: ct);

        // 페이드 아웃 (슬라이드 없이 제자리에서 사라짐)
        var fadeOutHandle = LMotion.Create(1f, 0f, param.SlideOutDuration)
            .WithEase(Ease.InCubic)
            .Bind(a => _canvasGroup.alpha = a);

        await UniTask.WaitUntil(() => !fadeOutHandle.IsActive(), cancellationToken: ct);

        _imageRect.gameObject.SetActive(false);
    }

    // 스프라이트를 최대 크기 내에서 비율 유지하며 맞춤
    private void FitImageSize(Sprite sprite)
    {
        float nativeW = sprite.rect.width;
        float nativeH = sprite.rect.height;

        float scale = Mathf.Min(_maxWidth / nativeW, _maxHeight / nativeH, 1f);

        _imageRect.sizeDelta = new Vector2(nativeW * scale, nativeH * scale);
    }
}