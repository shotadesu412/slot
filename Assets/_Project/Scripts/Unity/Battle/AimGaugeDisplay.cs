using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SlotRogue.Core.Character;
using SlotRogue.Core.Events;

namespace SlotRogue.Unity.Battle
{
    public class AimGaugeDisplay : MonoBehaviour
    {
        [SerializeField] private Image _gaugeFill;
        [SerializeField] private Button _aimButton;
        [SerializeField] private Button _overclockButton;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private Color _readyColor = Color.yellow;
        [SerializeField] private Color _normalColor = Color.cyan;

        private AimGauge _gauge;

        public void Initialize(AimGauge gauge)
        {
            _gauge = gauge;

            if (_aimButton != null)
            {
                _aimButton.onClick.AddListener(OnAimPressed);
                _aimButton.interactable = false;
            }

            if (_overclockButton != null)
                _overclockButton.onClick.AddListener(OnOverclockPressed);

            EventBus.Subscribe<AimGaugeChangedEvent>(OnGaugeChanged);
            EventBus.Subscribe<AimGaugeReadyEvent>(OnGaugeReady);
            EventBus.Subscribe<AimGaugeUsedEvent>(OnGaugeUsed);
            EventBus.Subscribe<OverclockActivatedEvent>(OnOverclock);

            UpdateVisual(0);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<AimGaugeChangedEvent>(OnGaugeChanged);
            EventBus.Unsubscribe<AimGaugeReadyEvent>(OnGaugeReady);
            EventBus.Unsubscribe<AimGaugeUsedEvent>(OnGaugeUsed);
            EventBus.Unsubscribe<OverclockActivatedEvent>(OnOverclock);
        }

        private void OnAimPressed()
        {
            if (_gauge == null || !_gauge.IsReady) return;
            _gauge.ActivateAim();
        }

        private void OnOverclockPressed()
        {
            if (_gauge == null) return;
            _gauge.ActivateOverclock();
        }

        private void OnGaugeChanged(AimGaugeChangedEvent evt)
        {
            UpdateVisual(evt.FillAmount);
        }

        private void OnGaugeReady(AimGaugeReadyEvent evt)
        {
            if (_aimButton != null)
                _aimButton.interactable = true;
            if (_gaugeFill != null)
                _gaugeFill.color = _readyColor;
            if (_statusText != null)
                _statusText.text = "AIM READY!";
        }

        private void OnGaugeUsed(AimGaugeUsedEvent evt)
        {
            if (_aimButton != null)
                _aimButton.interactable = false;
            if (_gaugeFill != null)
                _gaugeFill.color = _normalColor;
            if (_statusText != null)
                _statusText.text = "";
        }

        private void OnOverclock(OverclockActivatedEvent evt)
        {
            if (_statusText != null)
                _statusText.text = $"Speed x{evt.SpeedMultiplier:F1} / STR+{evt.StrengthGain}";
        }

        private void UpdateVisual(float fill)
        {
            if (_gaugeFill != null)
                _gaugeFill.fillAmount = fill;
        }
    }
}
