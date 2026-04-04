// Assets/Scripts/GamePlay/Player/PlayerAttack.cs
using UnityEngine;
using Reflex.Attributes;
using BulletPro;
using Cysharp.Threading.Tasks;
using System.Threading;
using FMODUnity;

public class PlayerAttack : MonoBehaviour
{
    [Inject] private IInputService _input;

    private PlayerModel _model;

    [Header("Emitter")]
    [SerializeField] private BulletEmitter _emitterTemplate;

    [Header("발사 사운드")]
    [SerializeField] private string _shootSoundPath = "event:/SFX/Player/Shoot";

    private BulletEmitter _mainEmitter;
    private Transform _emitterParent;

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
        if (_emitterTemplate != null)
        {
            _emitterParent = _emitterTemplate.transform.parent;
            _mainEmitter = _emitterTemplate;
            _emitterTemplate.gameObject.SetActive(false);
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
        await UniTask.WaitUntil(
            () => BulletPoolManager.instance != null,
            cancellationToken: token);

        if (token.IsCancellationRequested) return;

        if (_mainEmitter != null && _mainEmitter != _emitterTemplate)
        {
            Destroy(_mainEmitter.gameObject);
            _mainEmitter = null;
        }

        await UniTask.DelayFrame(1, cancellationToken: token);
        if (token.IsCancellationRequested) return;

        GameObject clone = Instantiate(
            _emitterTemplate.gameObject,
            _emitterParent);

        clone.name = "PlayerEmitter_Active";
        clone.transform.localPosition = _emitterTemplate.transform.localPosition;
        clone.transform.localRotation = _emitterTemplate.transform.localRotation;
        clone.SetActive(true);

        _mainEmitter = clone.GetComponent<BulletEmitter>();

        if (_model != null && _model.CurrentProfile != null)
            _mainEmitter.emitterProfile = _model.CurrentProfile;

        _isReady = true;
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

        if (!string.IsNullOrEmpty(_shootSoundPath))
            RuntimeManager.PlayOneShot(_shootSoundPath, transform.position);

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