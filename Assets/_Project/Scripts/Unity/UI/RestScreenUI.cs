using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SlotRogue.Core.Events;
using SlotRogue.Core.Map;
using SlotRogue.Unity.Bootstrap;

namespace SlotRogue.Unity.UI
{
    public class RestScreenUI : MonoBehaviour
    {
        [SerializeField] private Button _healButton;
        [SerializeField] private Button _leaveButton;
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private TextMeshProUGUI _healPreviewText;

        [Header("Settings")]
        [SerializeField] private float _healPercent = 0.3f;

        private RunState _runState;

        private void Start()
        {
            _runState = ServiceLocator.Get<RunState>();
            if (_runState == null) return;

            int healAmount = (int)(_runState.MaxHP * _healPercent);

            UpdateHPDisplay();

            if (_healPreviewText != null)
                _healPreviewText.text = $"HP {healAmount} 回復";

            if (_healButton != null)
                _healButton.onClick.AddListener(() => Heal(healAmount));

            if (_leaveButton != null)
                _leaveButton.onClick.AddListener(Leave);
        }

        private void Heal(int amount)
        {
            _runState.CurrentHP = System.Math.Min(_runState.MaxHP, _runState.CurrentHP + amount);
            UpdateHPDisplay();

            EventBus.Publish(new RestChoiceEvent { ChoseHeal = true });

            if (_healButton != null)
                _healButton.interactable = false;
        }

        private void UpdateHPDisplay()
        {
            if (_hpText != null && _runState != null)
                _hpText.text = $"HP: {_runState.CurrentHP}/{_runState.MaxHP}";
        }

        private void Leave()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Map");
        }
    }
}
