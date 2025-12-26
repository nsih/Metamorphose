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
        // 모델이 없으면 아무것도 못함
        if (_input == null || _mainEmitter == null || _model == null) return;

        // 공격 버튼 누름
        if (_input.IsAttackPressed) 
        {
            if (!_isShooting)
            {
                StartShooting();
            }
        }
        // 공격 버튼 뗌
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
            // 데미지 (Bullet Module -> Custom Params)
            rootBullet.moduleParameters.SetFloat(BPParams.Damage, _model.Damage);
            
            // 갈래 수 (Shot Module -> Layout -> Number)
            rootBullet.moduleParameters.SetFloat(BPParams.Count, _model.ProjectileCount);
            
            // 탄 퍼짐
            rootBullet.moduleParameters.SetFloat(BPParams.Spread, _model.SpreadAngle);
            
            // 탄속
            rootBullet.moduleParameters.SetFloat(BPParams.Speed, _model.SpeedScale);
            
            // 유도력
            rootBullet.moduleParameters.SetFloat(BPParams.Homing, _model.HomingStrength);
        }
        else
        {
            Debug.Log("프로필없");
        }
    }
}