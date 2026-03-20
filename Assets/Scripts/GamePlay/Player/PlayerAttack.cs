using UnityEngine;
using Reflex.Attributes;
using BulletPro;
using Cysharp.Threading.Tasks;
using System.Threading;

public class PlayerAttack : MonoBehaviour
{
    [Inject] private IInputService _input;

    private PlayerModel _model;

    [SerializeField] private BulletEmitter _mainEmitter;

    private bool _isShooting = false;
    private CancellationTokenSource _cts;

    [Inject]
    public void Construct(PlayerModel model)
    {
        _model = model;

        if (_mainEmitter != null && _model.CurrentProfile != null)
            _mainEmitter.emitterProfile = _model.CurrentProfile;
    }

    void OnEnable()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        _isShooting = false;
    }

    void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    void Update()
    {
        if (_input == null || _mainEmitter == null || _model == null) return;

        if (_input.IsAttackPressed)
        {
            if (!_isShooting)
                StartShooting();
        }
        else
        {
            if (_isShooting)
                StopShooting();
        }
    }

    public void StopAndReset()
    {
        _isShooting = false;

        if (_mainEmitter == null) return;
        if (_mainEmitter.bullets == null) return;
        if (_mainEmitter.bullets.Count == 0) return;

        try
        {
            _mainEmitter.Kill();
        }
        catch (System.Exception)
        {
            // 씬 전환 시 이미 파괴된 bullet 접근 무시
        }
    }

    private void StartShooting()
    {
        if (_model.CurrentProfile != _mainEmitter.emitterProfile)
            _mainEmitter.emitterProfile = _model.CurrentProfile;

        _mainEmitter.Play();
        _isShooting = true;

        ApplyDynamicStatsAsync(_cts.Token).Forget();
    }

    private void StopShooting()
    {
        _isShooting = false;
    }

    private async UniTaskVoid ApplyDynamicStatsAsync(CancellationToken token)
    {
        await UniTask.WaitUntil(
            () => _mainEmitter.rootBullet != null,
            cancellationToken: token);

        if (token.IsCancellationRequested) return;

        Bullet rootBullet = _mainEmitter.rootBullet;

        rootBullet.moduleParameters.SetFloat(BPParams.Damage, _model.Damage);
        rootBullet.moduleParameters.SetFloat(BPParams.Speed, _model.SpeedScale);
        rootBullet.moduleParameters.SetInt(BPParams.Count, _model.ProjectileCount);
        rootBullet.moduleParameters.SetFloat(BPParams.Spread, _model.SpreadAngle);
        rootBullet.moduleParameters.SetFloat(BPParams.Homing, _model.HomingStrength);
    }
}