using UnityEngine;
using Reflex.Attributes;

public class PlayerAim : MonoBehaviour
{
    [Inject] private IInputService _input;

    [Header("Components")]
    [SerializeField] private Transform _weaponPivot;

    private Camera _mainCam;

    void Update()
    {
        if (_input == null) return;

        // 씬 전환 후 카메라 교체 대응
        if (_mainCam == null)
            _mainCam = Camera.main;

        if (_mainCam == null) return;

        Vector3 mousePos = _mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        Vector3 direction = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _weaponPivot.rotation = Quaternion.Euler(0, 0, angle);
    }

    public Vector3 GetAimDirection()
    {
        return _weaponPivot.right;
    }
}