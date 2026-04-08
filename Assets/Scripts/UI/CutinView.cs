// Assets/Scripts/UI/CutinView.cs
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using LitMotion;

public class CutinView : MonoBehaviour, ICutinService
{
    [Serializable]
    public class CutinSlot
    {
        public RectTransform ImageRect;
        public Image CutinImage;
        public CanvasGroup CanvasGroup;
    }

    [Header("UI")]
    [SerializeField] private Canvas _canvas;

    [Header("우상단 슬롯 (Right)")]
    [SerializeField] private CutinSlot _rightSlot;

    [Header("좌하단 슬롯 (Left)")]
    [SerializeField] private CutinSlot _leftSlot;

    [Header("슬라이드 설정")]
    [SerializeField] private float _offscreenOffset = 1200f;
    [SerializeField] private float _maxWidth = 600f;
    [SerializeField] private float _maxHeight = 400f;
    [SerializeField] private float _margin = 40f;

    private void Awake()
    {
        if (_canvas != null)
        {
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 9000;
        }

        InitSlot(_rightSlot, new Vector2(1f, 1f));
        InitSlot(_leftSlot, new Vector2(0f, 0f));
    }

    private void InitSlot(CutinSlot slot, Vector2 anchor)
    {
        if (slot.ImageRect == null) return;

        slot.ImageRect.anchorMin = anchor;
        slot.ImageRect.anchorMax = anchor;
        slot.ImageRect.pivot = anchor;
        slot.CanvasGroup.alpha = 0f;
        slot.ImageRect.gameObject.SetActive(false);
    }

    public async UniTask ShowAsync(Sprite sprite, CutinDirection direction, CutinParams param, CancellationToken ct)
    {
        if (sprite == null) return;

        if (direction == CutinDirection.Right)
        {
            await ShowSlotAsync(_rightSlot, sprite, direction, param, ct);
        }
        else
        {
            await ShowSlotAsync(_leftSlot, sprite, direction, param, ct);
        }
    }

    private async UniTask ShowSlotAsync(CutinSlot slot, Sprite sprite, CutinDirection direction, CutinParams param, CancellationToken ct)
    {
        slot.CutinImage.sprite = sprite;
        FitImageSize(slot, sprite);
        slot.ImageRect.gameObject.SetActive(true);

        float startX;
        float endX;
        float yPos;

        if (direction == CutinDirection.Right)
        {
            // 우상단: 오른쪽 밖 → 안쪽으로
            startX = _offscreenOffset;
            endX = -_margin;
            yPos = -_margin;
        }
        else
        {
            // 좌하단: 왼쪽 밖 → 안쪽으로
            startX = -_offscreenOffset;
            endX = _margin;
            yPos = _margin;
        }

        slot.ImageRect.anchoredPosition = new Vector2(startX, yPos);
        slot.CanvasGroup.alpha = 0f;

        // 슬라이드 인 + 페이드 인
        var slideInHandle = LMotion.Create(startX, endX, param.SlideInDuration)
            .WithEase(Ease.OutCubic)
            .Bind(x =>
            {
                slot.ImageRect.anchoredPosition = new Vector2(x, yPos);
            });

        var fadeInHandle = LMotion.Create(0f, 1f, param.SlideInDuration * 0.5f)
            .Bind(a => slot.CanvasGroup.alpha = a);

        await UniTask.WaitUntil(() => !slideInHandle.IsActive(), cancellationToken: ct);

        // 유지
        await UniTask.Delay(
            (int)(param.HoldDuration * 1000),
            cancellationToken: ct);

        // 제자리 페이드 아웃
        var fadeOutHandle = LMotion.Create(1f, 0f, param.SlideOutDuration)
            .WithEase(Ease.InCubic)
            .Bind(a => slot.CanvasGroup.alpha = a);

        await UniTask.WaitUntil(() => !fadeOutHandle.IsActive(), cancellationToken: ct);

        slot.ImageRect.gameObject.SetActive(false);
    }

    private void FitImageSize(CutinSlot slot, Sprite sprite)
    {
        float nativeW = sprite.rect.width;
        float nativeH = sprite.rect.height;

        float scale = Mathf.Min(_maxWidth / nativeW, _maxHeight / nativeH, 1f);

        slot.ImageRect.sizeDelta = new Vector2(nativeW * scale, nativeH * scale);
    }
}