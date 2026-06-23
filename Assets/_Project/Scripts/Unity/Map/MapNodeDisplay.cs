using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SlotRogue.Core.Map;
using SlotRogue.Core.Events;

namespace SlotRogue.Unity.Map
{
    public class MapNodeDisplay : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private Image _background;

        [Header("Colors")]
        [SerializeField] private Color _battleColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color _eliteColor = new Color(0.9f, 0.5f, 0.1f);
        [SerializeField] private Color _shopColor = new Color(0.2f, 0.7f, 0.2f);
        [SerializeField] private Color _eventColor = new Color(0.2f, 0.5f, 0.9f);
        [SerializeField] private Color _restColor = new Color(0.4f, 0.8f, 0.4f);
        [SerializeField] private Color _bossColor = new Color(0.9f, 0.1f, 0.1f);
        [SerializeField] private Color _visitedColor = new Color(0.4f, 0.4f, 0.4f);

        private MapNode _node;

        public void Initialize(MapNode node)
        {
            _node = node;

            if (_label != null)
            {
                _label.text = node.Type switch
                {
                    FloorType.Battle => "Battle",
                    FloorType.Elite => "Elite",
                    FloorType.Shop => "Shop",
                    FloorType.Event => "Event",
                    FloorType.Rest => "Rest",
                    FloorType.Boss => "BOSS",
                    _ => "?"
                };
            }

            UpdateVisual();

            if (_button != null)
                _button.onClick.AddListener(OnClicked);
        }

        public void UpdateVisual()
        {
            if (_node == null) return;

            if (_background != null)
            {
                if (_node.IsVisited)
                    _background.color = _visitedColor;
                else
                    _background.color = GetColorForType(_node.Type);
            }

            if (_button != null)
                _button.interactable = _node.IsAccessible && !_node.IsVisited;
        }

        private Color GetColorForType(FloorType type)
        {
            return type switch
            {
                FloorType.Battle => _battleColor,
                FloorType.Elite => _eliteColor,
                FloorType.Shop => _shopColor,
                FloorType.Event => _eventColor,
                FloorType.Rest => _restColor,
                FloorType.Boss => _bossColor,
                _ => Color.white
            };
        }

        private void OnClicked()
        {
            if (_node == null || !_node.IsAccessible || _node.IsVisited) return;

            EventBus.Publish(new NodeSelectedEvent
            {
                NodeId = _node.Id,
                Type = _node.Type
            });
        }
    }
}
