using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using FMODUnity;
using GamePlay;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Yarn.Unity;

public class DialogueView : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _characterNameText;
    [SerializeField] private TextMeshProUGUI _lineText;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Transform _optionsContainer;
    [SerializeField] private Button _optionButtonPrefab;

    private DialogueBridge _bridge;
    private CompositeDisposable _disposables = new CompositeDisposable();
    private List<Button> _optionButtons = new List<Button>();
    private CancellationTokenSource _typingCts;
    private bool _isTyping = false;

    private void Awake()
    {
        _bridge = GetComponent<DialogueBridge>();

        if (_bridge == null)
        {
            Debug.LogError("DialogueView: DialogueBridge null");
            return;
        }

        _bridge.IsActive
            .Subscribe(active => _panel.SetActive(active))
            .AddTo(_disposables);

        _bridge.CharacterName
            .Subscribe(name => _characterNameText.text = name)
            .AddTo(_disposables);

        _bridge.CurrentLine
            .Subscribe(line => StartTyping(line))
            .AddTo(_disposables);

        _bridge.CurrentOptions
            .Subscribe(options => RefreshOptionButtons(options))
            .AddTo(_disposables);

        _continueButton.onClick.AddListener(OnContinueClicked);

        _panel.SetActive(false);
    }

    private void Update()
{
    if (_bridge == null) return;
    if (_optionButtons.Count > 0) return;
    if (!_panel.activeSelf) return;

    if (Keyboard.current.eKey.wasPressedThisFrame)
    {
        Debug.Log($"E 키 감지: isTyping={_isTyping}, isActive={_bridge.IsActive.Value}");
        OnContinueClicked();
    }
}

    private void StartTyping(string fullText)
    {
        _typingCts?.Cancel();
        _typingCts?.Dispose();
        _typingCts = new CancellationTokenSource();

        if (string.IsNullOrEmpty(fullText))
        {
            _lineText.text = string.Empty;
            _isTyping = false;
            return;
        }

        TypeAsync(fullText, _typingCts.Token).Forget();
    }

    private async UniTaskVoid TypeAsync(string fullText, CancellationToken token)
    {
        _isTyping = true;
        _lineText.text = string.Empty;

        for (int i = 0; i < fullText.Length; i++)
        {
            if (token.IsCancellationRequested)
            {
                _lineText.text = fullText;
                _isTyping = false;
                return;
            }

            _lineText.text += fullText[i];

            if (fullText[i] != ' ')
                RuntimeManager.PlayOneShot(FMODEvents.SFX.UI.DialogueTick);

            int delayMs = Mathf.RoundToInt(_bridge.TypingSpeed.Value * 1000f);
            await UniTask.Delay(delayMs, cancellationToken: token);
        }

        _isTyping = false;
    }

    private void OnContinueClicked()
    {
        if (_isTyping)
        {
            _typingCts?.Cancel();
            _lineText.text = _bridge.CurrentLine.Value;
            _isTyping = false;
            return;
        }

        if (!_bridge.IsActive.Value) return;

        _bridge.OnLineRead();
    }

    private void RefreshOptionButtons(List<DialogueOption> options)
    {
        foreach (var btn in _optionButtons)
            Destroy(btn.gameObject);
        _optionButtons.Clear();

        _continueButton.gameObject.SetActive(options.Count == 0);

        for (int i = 0; i < options.Count; i++)
        {
            int index = i;
            var option = options[i];

            var btn = Instantiate(_optionButtonPrefab, _optionsContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = option.Line.TextWithoutCharacterName.Text;
            btn.interactable = option.IsAvailable;
            btn.onClick.AddListener(() => _bridge.SelectOption(index));

            _optionButtons.Add(btn);
        }
    }

    private void OnDestroy()
    {
        _typingCts?.Cancel();
        _typingCts?.Dispose();
        _disposables.Dispose();
    }
}