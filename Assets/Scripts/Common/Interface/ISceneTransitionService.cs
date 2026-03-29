using System;
using Cysharp.Threading.Tasks;

public interface ISceneTransitionService
{
    UniTask FadeOut(float duration = 0.5f);
    UniTask FadeIn(float duration = 0.5f);
    UniTask TransitionAsync(Func<UniTask> loadAction, float fadeOutDuration = 0.5f, float fadeInDuration = 0.5f);
}