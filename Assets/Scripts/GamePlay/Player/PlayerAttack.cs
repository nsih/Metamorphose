using UnityEngine;
using Reflex.Attributes;
using BulletPro;

public class PlayerAttack : MonoBehaviour
{
    // [Dependency]
    [Inject] private IInputService _input;
    private PlayerModel _model; // 모델 주입 필요

    [SerializeField] private BulletEmitter _mainEmitter; 

    private bool _isShooting = false;

    // [Inject] 메서드를 통해 모델을 주입받고 초기화합니다.
    [Inject]
    public void Construct(PlayerModel model)
    {
        _model = model;
        
        // 시작하자마자 현재 모델의 무기 프로필을 적용해둡니다.
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

        if (_input.IsAttackPressed) 
        {
            if (!_isShooting)
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

        _mainEmitter.Play();
        _isShooting = true;

        ApplyDynamicStats();
    }

    private void StopShooting()
    {
        _mainEmitter.Stop();
        _isShooting = false;
    }

    private void ApplyDynamicStats()
    {
        Bullet rootBullet = _mainEmitter.rootBullet;

        if (rootBullet != null)
        {
            rootBullet.moduleParameters.SetFloat(BPParams.Damage, _model.Damage);
            Debug.Log(rootBullet.moduleParameters.GetFloat(BPParams.Damage));
            rootBullet.moduleParameters.SetFloat(BPParams.Speed, _model.SpeedScale);
            Debug.Log(rootBullet.moduleParameters.GetFloat(BPParams.Speed));
            
            rootBullet.moduleParameters.SetInt(BPParams.Count, _model.ProjectileCount);
            Debug.Log(rootBullet.moduleParameters.GetFloat(BPParams.Count));
            rootBullet.moduleParameters.SetFloat(BPParams.Spread, _model.SpreadAngle);
            Debug.Log(rootBullet.moduleParameters.GetFloat(BPParams.Spread));
            
            rootBullet.moduleParameters.SetFloat(BPParams.Homing, _model.HomingStrength);
            Debug.Log(rootBullet.moduleParameters.GetFloat(BPParams.Homing));

            Debug.Log("커스텀 파라메터 모델에 연결");
        }
        else
        {
            Debug.Log("프로필없");
        }
    }
}