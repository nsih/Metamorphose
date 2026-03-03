using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using Yarn.Unity;

// DialoguePresenterBase -> R3 ReactiveProperty 브릿지
public class DialogueBridge : DialoguePresenterBase
{
    // 현재 출력 중인 대사
    public ReactiveProperty<string> CurrentLine { get; } = new ReactiveProperty<string>(string.Empty);

    // 현재 화자 이름
    public ReactiveProperty<string> CharacterName { get; } = new ReactiveProperty<string>(string.Empty);

    // 현재 선택지 목록
    public ReactiveProperty<List<DialogueOption>> CurrentOptions { get; } = new ReactiveProperty<List<DialogueOption>>(new List<DialogueOption>());

    // 대화 패널 활성화 여부
    public ReactiveProperty<bool> IsActive { get; } = new ReactiveProperty<bool>(false);

    // View에서 다음 줄 진행 요청 신호
    private readonly Subject<Unit> _lineAdvanceSignal = new Subject<Unit>();

    // View에서 선택지 선택 신호 (선택 인덱스)
    private readonly Subject<int> _optionSelectedSignal = new Subject<int>();

    public override async YarnTask RunLineAsync(LocalizedLine dialogueLine, LineCancellationToken token)
    {
        CharacterName.Value = dialogueLine.CharacterName ?? string.Empty;
        CurrentLine.Value = dialogueLine.TextWithoutCharacterName.Text;
        CurrentOptions.Value = new List<DialogueOption>();
        IsActive.Value = true;

        // 버튼 입력 또는 Yarn 외부 진행 신호 중 먼저 오는 것 대기
        await UniTask.WhenAny(
            _lineAdvanceSignal.FirstAsync().AsUniTask(),
            UniTask.WaitUntilCanceled(token.NextLineToken)
        );
    }

    public override async YarnTask<DialogueOption> RunOptionsAsync(DialogueOption[] dialogueOptions, CancellationToken cancellationToken)
    {
        CurrentOptions.Value = new List<DialogueOption>(dialogueOptions);

        // 선택지 선택 대기
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

    // View에서 호출: 다음 줄 진행
    public void OnLineRead()
    {
        _lineAdvanceSignal.OnNext(Unit.Default);
    }

    // View에서 호출: 선택지 선택
    public void SelectOption(int optionIndex)
    {
        _optionSelectedSignal.OnNext(optionIndex);
    }
}