using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SlotRogue.Core.Events;
using SlotRogue.Core.Map;
using SlotRogue.Unity.Bootstrap;

namespace SlotRogue.Unity.Map
{
    public class MapSceneManager : MonoBehaviour
    {
        [SerializeField] private Transform _nodeContainer;
        [SerializeField] private GameObject _nodeDisplayPrefab;
        [SerializeField] private Transform _lineContainer;
        [SerializeField] private GameObject _linePrefab;

        [Header("Layout")]
        [SerializeField] private float _floorSpacing = 120f;
        [SerializeField] private float _columnSpacing = 150f;
        [SerializeField] private Vector2 _startOffset = new Vector2(0, -300f);

        private RunState _runState;
        private readonly Dictionary<int, MapNodeDisplay> _nodeDisplays = new();

        private void Start()
        {
            _runState = ServiceLocator.Get<RunState>();
            if (_runState == null) return;

            if (_runState.MapNodes == null)
            {
                var generator = new MapGenerator(_runState.Seed);
                _runState.MapNodes = generator.Generate();
            }

            BuildMap();

            EventBus.Subscribe<NodeSelectedEvent>(OnNodeSelected);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<NodeSelectedEvent>(OnNodeSelected);
        }

        private void BuildMap()
        {
            if (_runState.MapNodes == null) return;

            // Group nodes by floor
            var floorGroups = new Dictionary<int, List<MapNode>>();
            foreach (var node in _runState.MapNodes)
            {
                if (!floorGroups.ContainsKey(node.Floor))
                    floorGroups[node.Floor] = new List<MapNode>();
                floorGroups[node.Floor].Add(node);
            }

            // Create node displays
            foreach (var node in _runState.MapNodes)
            {
                if (_nodeDisplayPrefab == null || _nodeContainer == null) continue;

                var obj = Instantiate(_nodeDisplayPrefab, _nodeContainer);
                var display = obj.GetComponent<MapNodeDisplay>();
                if (display != null)
                {
                    display.Initialize(node);
                    _nodeDisplays[node.Id] = display;
                }

                // Position
                int nodesInFloor = floorGroups[node.Floor].Count;
                float xOffset = (node.Column - (nodesInFloor - 1) * 0.5f) * _columnSpacing;
                float yOffset = node.Floor * _floorSpacing;

                var rt = obj.GetComponent<RectTransform>();
                if (rt != null)
                    rt.anchoredPosition = _startOffset + new Vector2(xOffset, yOffset);
            }

            // Draw connection lines
            foreach (var node in _runState.MapNodes)
            {
                foreach (var connectedId in node.ConnectedNodeIds)
                {
                    DrawLine(node.Id, connectedId);
                }
            }
        }

        private void DrawLine(int fromId, int toId)
        {
            if (_linePrefab == null || _lineContainer == null) return;
            if (!_nodeDisplays.ContainsKey(fromId) || !_nodeDisplays.ContainsKey(toId)) return;

            var fromRT = _nodeDisplays[fromId].GetComponent<RectTransform>();
            var toRT = _nodeDisplays[toId].GetComponent<RectTransform>();
            if (fromRT == null || toRT == null) return;

            var lineObj = Instantiate(_linePrefab, _lineContainer);
            var lineRT = lineObj.GetComponent<RectTransform>();
            if (lineRT == null) return;

            var from = fromRT.anchoredPosition;
            var to = toRT.anchoredPosition;
            var mid = (from + to) / 2f;
            var diff = to - from;
            float dist = diff.magnitude;
            float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

            lineRT.anchoredPosition = mid;
            lineRT.sizeDelta = new Vector2(dist, 2f);
            lineRT.localRotation = Quaternion.Euler(0, 0, angle);
        }

        private void OnNodeSelected(NodeSelectedEvent evt)
        {
            _runState.VisitNode(evt.NodeId);

            // Refresh all displays
            foreach (var kvp in _nodeDisplays)
                kvp.Value.UpdateVisual();

            // Transition to appropriate scene based on floor type
            HandleNodeType(evt.Type, evt.NodeId);
        }

        private void HandleNodeType(FloorType type, int nodeId)
        {
            switch (type)
            {
                case FloorType.Battle:
                case FloorType.Elite:
                case FloorType.Boss:
                    // Load battle scene
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Battle");
                    break;
                case FloorType.Shop:
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Shop");
                    break;
                case FloorType.Rest:
                    // TODO: Rest scene
                    break;
                case FloorType.Event:
                    // TODO: Event scene
                    break;
            }
        }
    }
}
