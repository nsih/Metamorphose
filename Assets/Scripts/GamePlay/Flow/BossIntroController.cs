// Assets/Scripts/GamePlay/Flow/BossIntroController.cs
using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using R3;
using TJR.Core.Interface;

public class BossIntroController : MonoBehaviour
{
    private ReactiveProperty<BossController> _bossHandle;
    private IInputService _inputService;
    private TopDownCameraController _cameraController;
    private ICutinService _cutinService;
    private IAudioService _audioService;

    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    private CancellationTokenSource _introCts;

    [Inject]
    public void Construct(
        ReactiveProperty<BossController> bossHandle,
        IInputService inputService,
        TopDownCameraController cameraController,
        ICutinService cutinService,
        IAudioService audioService)
    {
        _bossHandle = bossHandle;
        _inputService = inputService;
        _cameraController = cameraController;
        _cutinService = cutinService;
        _audioService = audioService;
    }

    private void Start()
    {
        if (_bossHandle == null)
        {
            Debug.LogWarning("BossIntroController: bossHandle null");
            return;
        }

        _bossHandle
            .Subscribe(OnBossChanged)
            .AddTo(_disposables);
    }

    private void OnDestroy()
    {
        _introCts?.Cancel();
        _introCts?.Dispose();
        _disposables.Dispose();
    }

    private void OnBossChanged(BossController boss)
    {
        _introCts?.Cancel();
        _introCts?.Dispose();
        _introCts = null;

        if (boss == null) return;

        _introCts = new CancellationTokenSource();
        RunIntroAsync(boss, _introCts.Token).Forget();
    }

    private async UniTaskVoid RunIntroAsync(BossController boss, CancellationToken ct)
    {
        // 1프레임 대기 - BossController.Start() 완료 보장
        await UniTask.DelayFrame(1, cancellationToken: ct);

        if (boss == null) return;

        BossProfileSO profile = boss.Profile;

        // 인트로 없으면 즉시 전투 시작
        if (!profile.HasIntro)
        {
            boss.StartBattle();
            return;
        }

        try
        {
            // 1. BGM 정지
            _audioService.StopMusic(true);

            // 2. 입력 차단
            _inputService.SetEnabled(false);

            // 3. 카메라 팬
            await _cameraController.PanToAsync(
                boss.transform.position,
                profile.CameraPanDuration,
                ct);

            // 4. Yarn 대화 (향후 확장, 현재 스킵)
            // if (!string.IsNullOrEmpty(profile.IntroYarnNode)) { ... }

            // 5. 컷인 + BGM 변경
            if (!string.IsNullOrEmpty(profile.BossBGM))
            {
                _audioService.PlayMusic(profile.BossBGM);
            }

            var cutinParams = new CutinParams
            {
                SlideInDuration = 0.3f,
                HoldDuration = profile.CutinHoldDuration,
                SlideOutDuration = 0.2f
            };

            await _cutinService.ShowAsync(
                profile.CutinSprite,
                profile.CutinDirection,
                cutinParams,
                ct);

            // 6. 카메라 복귀
            _cameraController.ReturnToPlayer();
            await UniTask.Delay(300, cancellationToken: ct);

            // 7. 전투 시작
            boss.StartBattle();

            // 8. 입력 복원
            _inputService.SetEnabled(true);

            Debug.Log("BossIntroController: 인트로 완료");
        }
        catch (OperationCanceledException)
        {
            Debug.Log("BossIntroController: 인트로 취소");
        }
    }
}