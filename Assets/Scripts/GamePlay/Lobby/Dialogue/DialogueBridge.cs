using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using FMODUnity;
using GamePlay;
using R3;
using UnityEngine;
using Yarn.Unity;

public class DialogueBridge : DialoguePresenterBase
{
    public ReactiveProperty<string> CurrentLine { get; } = new ReactiveProperty<string>(string.Empty);
    public ReactiveProperty<string> CharacterName { get; } = new ReactiveProperty<string>(string.Empty);
    public ReactiveProperty<List<DialogueOption>> CurrentOptions { get; } = new ReactiveProperty<List<DialogueOption>>(new List<DialogueOption>());
    public ReactiveProperty<bool> IsActive { get; } = new ReactiveProperty<bool>(false);

    // 글자당 딜레이 (초 단위). Yarn 커맨드로 변경 가능
    public ReactiveProperty<float> TypingSpeed { get; } = new ReactiveProperty<float>(0.05f);

    private UniTaskCompletionSource _lineCompletionSource;
    private readonly Subject<int> _optionSelectedSignal = new Subject<int>();

    [YarnCommand("typing_speed")]
    public void SetTypingSpeed(float speed)
    {
        TypingSpeed.Value = speed;
    }

    public override async YarnTask RunLineAsync(LocalizedLine dialogueLine, LineCancellationToken token)
    {
        _lineCompletionSource = new UniTaskCompletionSource();

        // Yarn 자체 취소 토큰도 CompletionSource로 수렴 — WhenAny 제거
        token.NextContentToken.Register(() => _lineCompletionSource.TrySetResult());

        CharacterName.Value = dialogueLine.CharacterName ?? string.Empty;
        CurrentLine.Value = dialogueLine.TextWithoutCharacterName.Text;
        CurrentOptions.Value = new List<DialogueOption>();
        IsActive.Value = true;

        await _lineCompletionSource.Task;
    }

    public override async YarnTask<DialogueOption> RunOptionsAsync(DialogueOption[] dialogueOptions, CancellationToken cancellationToken)
    {
        CurrentOptions.Value = new List<DialogueOption>(dialogueOptions);

        int selectedIndex = await _optionSelectedSignal
            .FirstAsync()
            .AsUniTask()
            .AttachExternalCancellation(cancellationToken);

        CurrentOptions.Value = new List<DialogueOption>();

        if (selectedIndex < 0 || selectedIndex >= dialogueOptions.Length)
            return null;

        return dialogueOptions[selectedIndex];
    }

    public override YarnTask OnDialogueCompleteAsync()
    {
        Debug.Log("OnDialogueCompleteAsync 호출");
        IsActive.Value = false;
        CurrentLine.Value = string.Empty;
        CharacterName.Value = string.Empty;
        CurrentOptions.Value = new List<DialogueOption>();
        return YarnTask.CompletedTask;
    }

    public override YarnTask OnDialogueStartedAsync()
    {
        IsActive.Value = true;
        return YarnTask.CompletedTask;
    }

    public void OnLineRead()
    {
        _lineCompletionSource?.TrySetResult();
    }

    public void SelectOption(int optionIndex)
    {
        _optionSelectedSignal.OnNext(optionIndex);
    }
}