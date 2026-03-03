using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

// DialogueBridge의 ReactiveProperty를 구독해서 UI 갱신
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

    public void Initialize(DialogueBridge bridge)
    {
        _bridge = bridge;

        _bridge.IsActive
            .Subscribe(active => _panel.SetActive(active))
            .AddTo(_disposables);

        _bridge.CharacterName
            .Subscribe(name => _characterNameText.text = name)
            .AddTo(_disposables);

        _bridge.CurrentLine
            .Subscribe(line => _lineText.text = line)
            .AddTo(_disposables);

        _bridge.CurrentOptions
            .Subscribe(options => RefreshOptionButtons(options))
            .AddTo(_disposables);

        _continueButton.onClick.AddListener(OnContinueClicked);

        _panel.SetActive(false);
    }

    private void OnContinueClicked()
    {
        _bridge.OnLineRead();
    }

    private void RefreshOptionButtons(List<DialogueOption> options)
    {
        // 기존 버튼 정리
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
        _disposables.Dispose();
    }
}