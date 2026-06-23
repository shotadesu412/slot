using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SlotRogue.Core.Slot;
using SlotRogue.Data;

namespace SlotRogue.Unity.Battle
{
    public class ReelDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _symbolText;
        [SerializeField] private Image _symbolIcon;
        [SerializeField] private TextMeshProUGUI _reelLabel;
        [SerializeField] private Image _background;
        [SerializeField] private Color _spinningColor = new Color(0.3f, 0.3f, 0.3f);
        [SerializeField] private Color _stoppedColor = new Color(0.1f, 0.1f, 0.1f);

        private Reel _reel;
        private bool _isSpinning;
        private float _displayTimer;
        private int _displayIndex;

        public void Bind(Reel reel, int reelNumber)
        {
            _reel = reel;
            if (_reelLabel != null)
                _reelLabel.text = $"Reel {reelNumber + 1}";
            UpdateDisplay();
        }

        public void OnSpinStarted()
        {
            _isSpinning = true;
            _displayTimer = 0;
            if (_background != null)
                _background.color = _spinningColor;
        }

        public void OnReelStopped(SkillData result)
        {
            _isSpinning = false;
            if (_background != null)
                _background.color = _stoppedColor;

            if (result != null)
            {
                if (_symbolText != null)
                    _symbolText.text = result.skillName;
                if (_symbolIcon != null && result.icon != null)
                    _symbolIcon.sprite = result.icon;
            }
            else
            {
                if (_symbolText != null)
                    _symbolText.text = "---";
            }
        }

        private void Update()
        {
            if (!_isSpinning || _reel == null || _reel.Symbols.Count == 0) return;

            _displayTimer += Time.deltaTime;
            if (_displayTimer >= 0.08f)
            {
                _displayTimer = 0;
                _displayIndex = (_displayIndex + 1) % _reel.Symbols.Count;
                var symbol = _reel.Symbols[_displayIndex];
                if (_symbolText != null)
                    _symbolText.text = symbol.skillName;
            }
        }

        private void UpdateDisplay()
        {
            if (_reel == null || _reel.Symbols.Count == 0)
            {
                if (_symbolText != null)
                    _symbolText.text = "Empty";
                return;
            }

            var current = _reel.GetCurrentSymbol();
            if (_symbolText != null)
                _symbolText.text = current?.skillName ?? "---";
        }
    }
}
