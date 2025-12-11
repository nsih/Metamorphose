using UnityEngine;
using Reflex.Attributes;

public class PlayerAim : MonoBehaviour
{
    [Inject] private IInputService _input;

    [Header("Components")]
    [SerializeField] private Transform _weaponPivot;
    [SerializeField] private SpriteRenderer _weaponSprite;

    private Camera _mainCam;

    void Awake()
    {
        _mainCam = Camera.main;
    }

    void Update()
    {
        if (_input == null) return;

        Vector3 mousePos = _mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        Vector3 direction = (mousePos - transform.position).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        _weaponPivot.rotation = Quaternion.Euler(0, 0, angle);

        if (Mathf.Abs(angle) > 90)
        {
            _weaponSprite.flipY = true; // Y축 반전 (총이 똑바로 보임)
        }
        else
        {
            _weaponSprite.flipY = false;
        }
    }

    public Vector3 GetAimDirection()
    {
        return _weaponPivot.right;
    }
}