using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DashAfterimageController : MonoBehaviour
{
    [SerializeField] private Sprite[] _afterimageSprites;
    [SerializeField] private Color _afterimageColor = new Color(1f, 0.4f, 0.8f, 0.8f);

    private const float SpawnInterval = 0.05f;
    private const float FadeDuration = 0.3f;
    private const int PoolInitSize = 100;
    private const int PoolMaxSize = 200;

    private PlayerDash _playerDash;
    private SpriteRenderer _playerSpriteRenderer;

    private Queue<GameObject> _pool = new Queue<GameObject>();
    private CancellationTokenSource _spawnCts;

    void Awake()
    {
        _playerDash = GetComponent<PlayerDash>();
        _playerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        for (int i = 0; i < PoolInitSize; i++)
            _pool.Enqueue(CreateAfterimageObject());
    }

    void OnDestroy()
    {
        _spawnCts?.Cancel();
        _spawnCts?.Dispose();
    }

    void Update()
    {
        if (_playerDash == null) return;

        if (_playerDash.IsDashing && _spawnCts == null)
        {
            _spawnCts = new CancellationTokenSource();
            SpawnLoop(_spawnCts.Token).Forget();
        }
        else if (!_playerDash.IsDashing && _spawnCts != null)
        {
            _spawnCts.Cancel();
            _spawnCts.Dispose();
            _spawnCts = null;
        }
    }

    private async UniTaskVoid SpawnLoop(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                SpawnAfterimage();
                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(SpawnInterval),
                    ignoreTimeScale: true,
                    cancellationToken: token
                );
            }
        }
        catch (System.OperationCanceledException) { }
    }

    private void SpawnAfterimage()
    {
        if (_afterimageSprites == null || _afterimageSprites.Length == 0) return;

        GameObject obj = GetFromPool();
        if (obj == null) return;

        obj.transform.position = transform.position;
        obj.transform.rotation = transform.rotation;
        obj.SetActive(true);

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        int randomIndex = Random.Range(0, _afterimageSprites.Length);
        sr.sprite = _afterimageSprites[randomIndex];
        sr.flipX = _playerSpriteRenderer != null && _playerSpriteRenderer.flipX;
        sr.color = _afterimageColor;

        if (_playerSpriteRenderer != null)
            sr.sortingOrder = _playerSpriteRenderer.sortingOrder - 1;

        FadeAndReturn(obj, sr, this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid FadeAndReturn(GameObject obj, SpriteRenderer sr, CancellationToken token)
    {
        float elapsed = 0f;
        Color startColor = sr.color;

        try
        {
            while (elapsed < FadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / FadeDuration);
                var c = startColor;
                c.a = alpha;
                sr.color = c;
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        catch (System.OperationCanceledException) { }

        ReturnToPool(obj);
    }

    private GameObject GetFromPool()
    {
        if (_pool.Count > 0)
            return _pool.Dequeue();

        // 풀 고갈 시 최대치 미만이면 신규 생성
        if (CountAllAfterimages() < PoolMaxSize)
            return CreateAfterimageObject();

        Debug.LogWarning("DashAfterimageController: 풀 고갈");
        return null;
    }

    private void ReturnToPool(GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);
        _pool.Enqueue(obj);
    }

    private GameObject CreateAfterimageObject()
    {
        var obj = new GameObject("Afterimage");
        obj.AddComponent<SpriteRenderer>();
        obj.SetActive(false);
        return obj;
    }

    private int CountAllAfterimages()
    {
        // 풀 내부 + 현재 활성 오브젝트 합산 추정 (최대치 guard용)
        return PoolMaxSize; // 단순화: 최대치 초과 방지만 목적
    }
}