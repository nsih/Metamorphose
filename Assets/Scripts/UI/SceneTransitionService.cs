using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SceneTransitionService : MonoBehaviour, ISceneTransitionService
{
    private Image _fadeImage;

    public void Initialize()
    {
        DontDestroyOnLoad(gameObject);
        CreateFadeCanvas();
    }

    private void CreateFadeCanvas()
    {
        var canvasGo = new GameObject("FadeCanvas");
        canvasGo.transform.SetParent(transform);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        var imageGo = new GameObject("FadeImage");
        imageGo.transform.SetParent(canvasGo.transform, false);

        _fadeImage = imageGo.AddComponent<Image>();
        _fadeImage.color = Color.black;
        _fadeImage.raycastTarget = false;

        var rect = imageGo.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        SetAlpha(0f);
    }

    private void SetAlpha(float alpha)
    {
        var color = _fadeImage.color;
        color.a = alpha;
        _fadeImage.color = color;
    }

    public async UniTask FadeOut(float duration = 0.5f)
    {
        _fadeImage.raycastTarget = true;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Clamp01(elapsed / duration));
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        SetAlpha(1f);
    }

    public async UniTask FadeIn(float duration = 0.5f)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(1f - Mathf.Clamp01(elapsed / duration));
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        SetAlpha(0f);
        _fadeImage.raycastTarget = false;
    }

    public async UniTask TransitionAsync(Func<UniTask> loadAction, float fadeOutDuration = 0.5f, float fadeInDuration = 0.5f)
    {
        await FadeOut(fadeOutDuration);
        await loadAction();
        await FadeIn(fadeInDuration);
    }
}