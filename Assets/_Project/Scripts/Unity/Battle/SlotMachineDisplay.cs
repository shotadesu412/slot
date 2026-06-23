using UnityEngine;
using UnityEngine.UI;
using SlotRogue.Core.Events;
using SlotRogue.Core.Battle;

namespace SlotRogue.Unity.Battle
{
    public class SlotMachineDisplay : MonoBehaviour
    {
        [SerializeField] private ReelDisplay[] _reelDisplays = new ReelDisplay[3];
        [SerializeField] private Button[] _stopButtons = new Button[3];
        [SerializeField] private Button _spinButton;
        [SerializeField] private Button _endTurnButton;

        private BattleController _battleController;

        public void Initialize(BattleController controller)
        {
            _battleController = controller;

            for (int i = 0; i < 3; i++)
            {
                if (_reelDisplays[i] != null)
                    _reelDisplays[i].Bind(controller.SlotMachine.Reels[i], i);

                int index = i;
                if (_stopButtons[i] != null)
                {
                    _stopButtons[i].onClick.AddListener(() => OnStopButtonPressed(index));
                    _stopButtons[i].gameObject.SetActive(false);
                }
            }

            if (_spinButton != null)
                _spinButton.onClick.AddListener(OnSpinButtonPressed);

            if (_endTurnButton != null)
                _endTurnButton.onClick.AddListener(OnEndTurnPressed);

            EventBus.Subscribe<SpinStartedEvent>(OnSpinStarted);
            EventBus.Subscribe<ReelStoppedEvent>(OnReelStopped);
            EventBus.Subscribe<AllReelsStoppedEvent>(OnAllReelsStopped);
            EventBus.Subscribe<SpinsChangedEvent>(OnSpinsChanged);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<SpinStartedEvent>(OnSpinStarted);
            EventBus.Unsubscribe<ReelStoppedEvent>(OnReelStopped);
            EventBus.Unsubscribe<AllReelsStoppedEvent>(OnAllReelsStopped);
            EventBus.Unsubscribe<SpinsChangedEvent>(OnSpinsChanged);
        }

        private void OnSpinButtonPressed()
        {
            if (_battleController.TryStartSpin())
            {
                if (_spinButton != null)
                    _spinButton.interactable = false;
            }
        }

        private void OnStopButtonPressed(int index)
        {
            _battleController.TryStopReel(index);
        }

        private void OnEndTurnPressed()
        {
            _battleController.EndTurnManually();
        }

        private void OnSpinStarted(SpinStartedEvent evt)
        {
            foreach (var display in _reelDisplays)
                if (display != null) display.OnSpinStarted();

            for (int i = 0; i < 3; i++)
                if (_stopButtons[i] != null)
                {
                    _stopButtons[i].gameObject.SetActive(true);
                    _stopButtons[i].interactable = true;
                }
        }

        private void OnReelStopped(ReelStoppedEvent evt)
        {
            if (evt.ReelIndex >= 0 && evt.ReelIndex < 3)
            {
                var skill = _battleController.SlotMachine.Reels[evt.ReelIndex].GetCurrentSymbol();
                _reelDisplays[evt.ReelIndex]?.OnReelStopped(skill);

                if (_stopButtons[evt.ReelIndex] != null)
                    _stopButtons[evt.ReelIndex].interactable = false;
            }
        }

        private void OnAllReelsStopped(AllReelsStoppedEvent evt)
        {
            for (int i = 0; i < 3; i++)
                if (_stopButtons[i] != null)
                    _stopButtons[i].gameObject.SetActive(false);

            UpdateSpinButton();
        }

        private void OnSpinsChanged(SpinsChangedEvent evt)
        {
            UpdateSpinButton();
        }

        private void UpdateSpinButton()
        {
            if (_spinButton != null)
            {
                bool canSpin = _battleController.State == BattleState.WaitingForSpin
                    && _battleController.TurnManager.HasSpins;
                _spinButton.interactable = canSpin;
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (_spinButton != null)
                _spinButton.interactable = interactable;
            if (_endTurnButton != null)
                _endTurnButton.interactable = interactable;
        }
    }
}
