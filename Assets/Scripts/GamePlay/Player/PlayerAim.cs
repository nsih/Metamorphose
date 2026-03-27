using UnityEngine;
using UnityEngine.InputSystem;
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

        if (_mainCam == null)
            _mainCam = Camera.main;

        if (_mainCam == null) return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mousePos = _mainCam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 0f));
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