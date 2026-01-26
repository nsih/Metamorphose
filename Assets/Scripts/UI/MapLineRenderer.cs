using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Common;

public class MapLineRenderer : MonoBehaviour
{
    [SerializeField] private float _lineWidth = 3f;
    [SerializeField] private Color _activeLineColor = Color.white;
    [SerializeField] private Color _inactiveLineColor = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color _lockedLineColor = new Color(0.2f, 0.2f, 0.2f);
    [SerializeField] private Sprite _lineSprite;

    private List<GameObject> _lineObjects = new List<GameObject>();

    public void DrawLines(Dictionary<MapNode, RectTransform> nodePositions, Map map)
    {
        ClearLines();

        if (map == null || map.LayerCount == 0) return;

        for(int layer = 0; layer < map.LayerCount; layer++)
        {
            var layerNodes = map.GetNodesInLayer(layer);
            foreach (var node in layerNodes)
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

                    Color lineColor = GetLineColor(node, nextNode);
                    DrawLine(fromPos, toPos, lineColor);
                }
            }
        }
    }

    private Color GetLineColor(MapNode fromNode, MapNode toNode)
    {
        // 시작 노드가 Completed이고 다음 노드가 Locked이면 검은색
        if (fromNode.State == NodeState.Completed && toNode.State == NodeState.Locked)
        {
            return _lockedLineColor;
        }
        
        // 시작 노드가 Completed이면 흰색 (활성화된 경로)
        if (fromNode.State == NodeState.Completed)
        {
            return _activeLineColor;
        }
        
        // 시작 노드가 Available이고 다음 노드가 Available이면 회색
        if (fromNode.State == NodeState.Available && toNode.State == NodeState.Available)
        {
            return _inactiveLineColor;
        }
        
        // 나머지는 검은색
        return _lockedLineColor;
    }

    private void DrawLine(Vector2 from, Vector2 to, Color color)
    {
        GameObject lineObj = new GameObject("MapLine");
        lineObj.transform.SetParent(transform, false);

        RectTransform rect = lineObj.AddComponent<RectTransform>();
        Image image = lineObj.AddComponent<Image>();

        if (_lineSprite != null)
        {
            image.sprite = _lineSprite;
        }
        image.color = color;

        Vector2 direction = to - from;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        rect.anchoredPosition = (from + to) / 2f;
        rect.sizeDelta = new Vector2(distance, _lineWidth);
        rect.localRotation = Quaternion.Euler(0, 0, angle);
        rect.pivot = new Vector2(0.5f, 0.5f);

        _lineObjects.Add(lineObj);
    }

    public void ClearLines()
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