// Assets/Scripts/GamePlay/Player/Camera/TopDownCameraController.cs
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System.Threading;
using Reflex.Attributes;

public class TopDownCameraController : MonoBehaviour
{
    [Header("Mouse Offset Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float _mouseLookWeight = 0.3f;
    [SerializeField] private float _maxMouseOffset = 5f;

    [Header("Smoothing")]
    [SerializeField] private float _smoothSpeed = 5f;

    [Header("Camera Settings")]
    [SerializeField] private float _cameraHeight = -10f;

    private Camera _camera;
    private Vector3 _velocity = Vector3.zero;
    private Transform _playerTransform;
    private PlayerSpawner _playerSpawner;

    // 팬 연출
    private bool _isPanning;
    private Vector3 _panTarget;

    [Inject]
    public void Construct(PlayerSpawner spawner)
    {
        _playerSpawner = spawner;
    }

    void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    void Start()
    {
        if (_playerSpawner != null)
        {
            GameObject playerObj = _playerSpawner.Spawn();

            if (playerObj != null)
            {
                _playerTransform = playerObj.transform;
            }
        }
        else
        {
            Debug.LogError("TopDownCameraController: PlayerSpawner null");
        }
    }

    void LateUpdate()
    {
        if (_isPanning)
        {
            Vector3 target = new Vector3(_panTarget.x, _panTarget.y, _cameraHeight);
            Vector3 smoothed = Vector3.SmoothDamp(
                transform.position, target, ref _velocity, 1f / _smoothSpeed);
            transform.position = smoothed;
            return;
        }

        if (_playerTransform == null) return;

        Vector3 targetPosition = CalculateTargetPosition();
        Vector3 smoothedPosition = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _velocity,
            1f / _smoothSpeed
        );

        transform.position = smoothedPosition;
    }

    private Vector3 CalculateTargetPosition()
    {
        Vector3 playerPos = _playerTransform.position;
        Vector3 mouseWorldPos = GetMouseWorldPosition();

        Vector3 mouseOffset = (mouseWorldPos - playerPos);
        mouseOffset = Vector3.ClampMagnitude(mouseOffset, _maxMouseOffset);

        Vector3 targetPos = playerPos + (mouseOffset * _mouseLookWeight);
        targetPos.z = _cameraHeight;

        return targetPos;
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (_camera == null) return _playerTransform.position;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseScreenPos = new Vector3(mouseScreen.x, mouseScreen.y, Mathf.Abs(_cameraHeight));

        return _camera.ScreenToWorldPoint(mouseScreenPos);
    }

    /// <summary>
    /// 지정 위치로 카메라 팬. duration 동안 SmoothDamp로 이동
    /// </summary>
    public async UniTask PanToAsync(Vector3 worldTarget, float duration = 0.6f,
                                     CancellationToken ct = default)
    {
        _isPanning = true;
        _panTarget = worldTarget;
        _velocity = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (ct.IsCancellationRequested) break;
            elapsed += Time.deltaTime;
            await UniTask.Yield(ct);
        }

        _isPanning = false;
    }

    /// <summary>
    /// 팬 중단, 플레이어 추적 복귀
    /// </summary>
    public void ReturnToPlayer()
    {
        _isPanning = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (_playerTransform == null || !Application.isPlaying) return;

        Vector3 playerPos = _playerTransform.position;
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Vector3 targetPos = CalculateTargetPosition();

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(playerPos, 0.5f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(playerPos, mouseWorldPos);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(targetPos, 0.3f);
        Gizmos.DrawLine(transform.position, targetPos);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerPos, _maxMouseOffset);
    }
}