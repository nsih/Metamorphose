using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class BulletTimeVisualController : MonoBehaviour
{
    [Header("color invert")]
    [SerializeField] private RawImage _invertImage;
    [SerializeField] private float _invertFadeIn = 0.05f;
    [SerializeField] private float _invertFadeOut = 0.3f;
    [SerializeField] private float _invertMaxAlpha = 0.3f;

    [Header("red noise")]
    [SerializeField] private RawImage _noiseImage;
    [SerializeField] private float _noiseFadeIn = 0.05f;
    [SerializeField] private float _noiseFadeOut = 0.3f;
    [SerializeField] private float _noiseMaxAlpha = 0.4f;

    [Header("silhouette")]
    [SerializeField] private Image[] _silhouetteImages;
    [SerializeField] private Sprite[] _silhouetteSprites;
    [SerializeField] private float _silhouetteFadeOut = 0.15f;

    private BulletTimeManager _bulletTimeManager;
    private CancellationTokenSource _cts;
    private CancellationTokenSource _silhouetteCts;

    void Awake()
    {
        _bulletTimeManager = GetComponent<BulletTimeManager>();

        SetAlpha(_invertImage, 0f);
        SetAlpha(_noiseImage, 0f);

        if (_silhouetteImages != null)
            foreach (var img in _silhouetteImages)
                SetAlpha(img, 0f);
    }

    void Start()
    {
        if (_bulletTimeManager == null)
        {
            Debug.LogError("BulletTimeVisualController: BulletTimeManager null");
            return;
        }

        _bulletTimeManager.OnBulletTimeStart += HandleStart;
        _bulletTimeManager.OnBulletTimeEnd += HandleEnd;
    }

    void OnDestroy()
    {
        if (_bulletTimeManager != null)
        {
            _bulletTimeManager.OnBulletTimeStart -= HandleStart;
            _bulletTimeManager.OnBulletTimeEnd -= HandleEnd;
        }
        Cancel();
    }

    private void HandleStart()
    {
        Cancel();
        _cts = new CancellationTokenSource();
        PlayStart(_cts.Token).Forget();
    }

    private void HandleEnd()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        _cts = new CancellationTokenSource();
        PlayEnd(_cts.Token).Forget();

        FadeOutSilhouettes().Forget();
    }

    private async UniTaskVoid PlayStart(CancellationToken token)
    {
        try
        {
            await UniTask.WhenAll(
                FadeRawImage(_invertImage, 0f, _invertMaxAlpha, _invertFadeIn, token),
                FadeRawImage(_noiseImage, 0f, _noiseMaxAlpha, _noiseFadeIn, token),
                ShowSilhouettes(token)
            );
        }
        catch (OperationCanceledException) { }
    }

    private async UniTaskVoid PlayEnd(CancellationToken token)
    {
        try
        {
            await UniTask.WhenAll(
                FadeRawImage(_invertImage, _invertMaxAlpha, 0f, _invertFadeOut, token),
                FadeRawImage(_noiseImage, _noiseMaxAlpha, 0f, _noiseFadeOut, token)
            );
        }
        catch (OperationCanceledException) { }
    }

    private async UniTaskVoid FadeOutSilhouettes()
    {
        CancelSilhouette();
        _silhouetteCts = new CancellationTokenSource();
        await FadeImages(_silhouetteImages, 1f, 0f, _silhouetteFadeOut, _silhouetteCts.Token);
    }

    private async UniTask FadeRawImage(RawImage image, float from, float to, float duration, CancellationToken token)
    {
        if (image == null) return;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(image, Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        SetAlpha(image, to);
    }

    private async UniTask FadeImages(Image[] images, float from, float to, float duration, CancellationToken token)
    {
        if (images == null) return;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            foreach (var img in images)
                if (img != null) SetAlpha(img, alpha);
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        foreach (var img in images)
            if (img != null) SetAlpha(img, to);
    }

    private async UniTask ShowSilhouettes(CancellationToken token)
    {
        if (_silhouetteImages == null || _silhouetteImages.Length == 0) return;
        if (_silhouetteSprites == null || _silhouetteSprites.Length == 0) return;

        CancelSilhouette();
        _silhouetteCts = new CancellationTokenSource();

        foreach (var img in _silhouetteImages)
        {
            if (img == null) continue;
            img.sprite = _silhouetteSprites[UnityEngine.Random.Range(0, _silhouetteSprites.Length)];
            img.rectTransform.anchoredPosition = new Vector2(
                UnityEngine.Random.Range(-800f, 800f),
                UnityEngine.Random.Range(-400f, 400f)
            );
            img.rectTransform.sizeDelta = new Vector2(200f, 200f);
            SetAlpha(img, 1f);
        }

        await UniTask.WaitUntil(() => !_bulletTimeManager.IsBulletTimeActive,
            PlayerLoopTiming.Update, token);
    }

    private void SetAlpha(RawImage image, float alpha)
    {
        if (image == null) return;
        var c = image.color;
        c.a = alpha;
        image.color = c;
    }

    private void SetAlpha(Image image, float alpha)
    {
        if (image == null) return;
        var c = image.color;
        c.a = alpha;
        image.color = c;
    }

    private void CancelSilhouette()
    {
        if (_silhouetteCts != null)
        {
            _silhouetteCts.Cancel();
            _silhouetteCts.Dispose();
            _silhouetteCts = null;
        }
    }

    private void Cancel()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
        CancelSilhouette();
    }
}