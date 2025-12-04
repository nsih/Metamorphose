using UnityEngine;
using Reflex.Attributes;
using BulletPro;

public class PlayerAttack : MonoBehaviour
{
    [Inject] private IInputService _input;

    [SerializeField] private BulletEmitter _mainEmitter; 

    private bool _isShooting = false;

    void Start()
    {
        // 시작 시 안전하게 끄기
        if (_mainEmitter != null) _mainEmitter.Kill();
    }

    void Update()
    {
        if (_input == null || _mainEmitter == null) return;

        if (_input.IsAttackPressed) 
        {
            if (!_isShooting)
            {
                _mainEmitter.Play();
                _isShooting = true;

                //Debug.Log("S");
            }
        }
        else
        {
            if (_isShooting)
            {
                _mainEmitter.Stop();
                _isShooting = false;

                //Debug.Log("E");
            }
        }
    }
}