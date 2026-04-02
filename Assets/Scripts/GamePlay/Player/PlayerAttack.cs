// Assets/Scripts/GamePlay/Player/PlayerAttack.cs
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

    // emitter가 붙어있는 자식 오브젝트의 로컬 위치/부모를 기억
    private Transform _emitterParent;
    private Vector3 _emitterLocalPos;
    private Quaternion _emitterLocalRot;

    private bool _isShooting = false;
    private bool _isReady = false;
    private CancellationTokenSource _cts;

    [Inject]
    public void Construct(PlayerModel model)
    {
        _model = model;
    }

    void Awake()
    {
        if (_mainEmitter != null)
        {
            _emitterParent = _mainEmitter.transform.parent;
            _emitterLocalPos = _mainEmitter.transform.localPosition;
            _emitterLocalRot = _mainEmitter.transform.localRotation;
        }
    }

    void OnEnable()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        _isShooting = false;
        _isReady = false;

        RebuildEmitterAsync(_cts.Token).Forget();
    }

    private async UniTaskVoid RebuildEmitterAsync(CancellationToken token)
    {
        // BulletPoolManager 준비 대기
        await UniTask.WaitUntil(
            () => BulletPoolManager.instance != null,
            cancellationToken: token);

        if (token.IsCancellationRequested) return;

        // 기존 emitter 오브젝트 파괴
        if (_mainEmitter != null)
        {
            Destroy(_mainEmitter.gameObject);
            _mainEmitter = null;
        }

        await UniTask.DelayFrame(1, cancellationToken: token);
        if (token.IsCancellationRequested) return;

        // 새 emitter 오브젝트 생성
        var emitterGo = new GameObject("PlayerEmitter");
        emitterGo.transform.SetParent(_emitterParent);
        emitterGo.transform.localPosition = _emitterLocalPos;
        emitterGo.transform.localRotation = _emitterLocalRot;

        _mainEmitter = emitterGo.AddComponent<BulletEmitter>();

        if (_model != null && _model.CurrentProfile != null)
            _mainEmitter.emitterProfile = _model.CurrentProfile;

        _isReady = true;
        Debug.Log("PlayerAttack: emitter 재생성 완료");
    }

    void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    void Update()
    {
        if (!_isReady) return;
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
        _isReady = false;

        if (_mainEmitter == null) return;

        try
        {
            _mainEmitter.Kill();
        }
        catch (System.Exception) { }
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
        _mainEmitter.Stop();
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