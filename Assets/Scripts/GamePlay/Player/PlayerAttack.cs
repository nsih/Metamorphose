using UnityEngine;
using Reflex.Attributes;
using BulletPro;

public class PlayerAttack : MonoBehaviour
{
    // [Dependency]
    [Inject] private IInputService _input;
    private PlayerModel _model; 

    [SerializeField] private BulletEmitter _mainEmitter; 

    private bool _isShooting = false;
    private float _fireTimer = 0f;

    [Inject]
    public void Construct(PlayerModel model)
    {
        _model = model;
        
        if (_mainEmitter != null && _model.CurrentProfile != null)
        {
            _mainEmitter.emitterProfile = _model.CurrentProfile;
        }
    }

    void Start()
    {
        if (_mainEmitter != null) _mainEmitter.Kill();
    }

    void Update()
    {
        if (_input == null || _mainEmitter == null || _model == null) return;

        if (_fireTimer > 0)
        {
            _fireTimer -= Time.deltaTime;
        }

        if (_input.IsAttackPressed) 
        {
            if (_fireTimer <= 0)
            {
                StartShooting();
            }
        }
        else
        {
            if (_isShooting)
            {
                StopShooting();
            }
        }
    }

    private void StartShooting()
    {
        if (_model.CurrentProfile != _mainEmitter.emitterProfile)
        {
            _mainEmitter.emitterProfile = _model.CurrentProfile;
        }

        //_mainEmitter.Kill(); 
        _mainEmitter.Play();
        
        _isShooting = true;

        ApplyDynamicStats();

        _fireTimer = _model.FireRate > 0 ? _model.FireRate : 0.05f;
    }

    private void StopShooting()
    {
        _isShooting = false;
    }

    private void ApplyDynamicStats()
    {
        Bullet rootBullet = _mainEmitter.rootBullet;

        if (rootBullet != null)
        {
            rootBullet.moduleParameters.SetFloat(BPParams.Damage, _model.Damage);
            rootBullet.moduleParameters.SetFloat(BPParams.Speed, _model.SpeedScale);
            rootBullet.moduleParameters.SetInt(BPParams.Count, _model.ProjectileCount);
            rootBullet.moduleParameters.SetFloat(BPParams.Spread, _model.SpreadAngle);
            rootBullet.moduleParameters.SetFloat(BPParams.Homing, _model.HomingStrength);
        }
        else
        {
            Debug.LogError("player emitter profile error");
        }
    }
}