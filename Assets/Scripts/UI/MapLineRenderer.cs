using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Common;

public class MapLineRenderer : MonoBehaviour
{
    [SerializeField] private float _lineWidth = 3f;
    [SerializeField] private Color _activeLineColor = Color.white;
    [SerializeField] private Color _inactiveLineColor = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Sprite _lineSprite;

    private List<GameObject> _lineObjects = new List<GameObject>();

    public void DrawLines(Dictionary<MapNode, RectTransform> nodePositions, List<List<MapNode>> grid)
    {
        ClearLines();

        if (grid == null || grid.Count == 0) return;

        foreach (var layer in grid)
        {
            foreach (var node in layer)
            {
                if (node.NextNodes == null || node.NextNodes.Count == 0) continue;
                if (!nodePositions.ContainsKey(node)) continue;

                RectTransform fromRect = nodePositions[node];
                Vector3 fromPos = fromRect.anchoredPosition;

                foreach (var nextNode in node.NextNodes)
                {
                    if (!nodePositions.ContainsKey(nextNode)) continue;

                    RectTransform toRect = nodePositions[nextNode];
                    Vector3 toPos = toRect.anchoredPosition;

                    DrawLine(fromPos, toPos, node.State == NodeState.Completed);
                }
            }
        }
    }

    private void DrawLine(Vector2 from, Vector2 to, bool isActive)
    {
        GameObject lineObj = new GameObject("MapLine");
        lineObj.transform.SetParent(transform, false);

        RectTransform rect = lineObj.AddComponent<RectTransform>();
        Image image = lineObj.AddComponent<Image>();

        // 선 스프라이트 설정 (없으면 기본 흰색)
        if (_lineSprite != null)
        {
            image.sprite = _lineSprite;
        }
        image.color = isActive ? _activeLineColor : _inactiveLineColor;

        // 두 점 사이의 거리와 각도 계산
        Vector2 direction = to - from;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 위치: 두 점의 중간
        rect.anchoredPosition = (from + to) / 2f;

        // 크기: 거리 x 선 두께
        rect.sizeDelta = new Vector2(distance, _lineWidth);

        // 회전: 두 점을 잇는 각도
        rect.localRotation = Quaternion.Euler(0, 0, angle);

        // 피벗을 중앙으로
        rect.pivot = new Vector2(0.5f, 0.5f);

        _lineObjects.Add(lineObj);
    }

    private void ClearLines()
    {
        foreach (var lineObj in _lineObjects)
        {
            if (lineObj != null)
            {
                Destroy(lineObj);
            }
        }
        _lineObjects.Clear();
    }

    private void OnDestroy()
    {
        ClearLines();
    }
}