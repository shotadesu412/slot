using UnityEngine;
using TMPro;
using SlotRogue.Core.Events;
using SlotRogue.Unity.Common;

namespace SlotRogue.Unity.Battle
{
    public class PlayerHUD : MonoBehaviour
    {
        [SerializeField] private HPBar _hpBar;
        [SerializeField] private TextMeshProUGUI _blockText;
        [SerializeField] private TextMeshProUGUI _spinCountText;
        [SerializeField] private TextMeshProUGUI _turnText;
        [SerializeField] private TextMeshProUGUI _statusText;

        private int _maxHP;

        public void Initialize(int maxHP)
        {
            _maxHP = maxHP;
            if (_hpBar != null)
                _hpBar.Initialize(maxHP);
            UpdateBlock(0);
            UpdateSpins(0);

            EventBus.Subscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Subscribe<HealEvent>(OnHeal);
            EventBus.Subscribe<BlockGainedEvent>(OnBlockGained);
            EventBus.Subscribe<SpinsChangedEvent>(OnSpinsChanged);
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<StatusAppliedEvent>(OnStatusApplied);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Unsubscribe<HealEvent>(OnHeal);
            EventBus.Unsubscribe<BlockGainedEvent>(OnBlockGained);
            EventBus.Unsubscribe<SpinsChangedEvent>(OnSpinsChanged);
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<StatusAppliedEvent>(OnStatusApplied);
        }

        private void OnDamageDealt(DamageDealtEvent evt)
        {
            if (evt.IsPlayer && _hpBar != null)
                _hpBar.SetHP(evt.RemainingHP);
        }

        private void OnHeal(HealEvent evt)
        {
            if (evt.IsPlayer && _hpBar != null)
                _hpBar.SetHP(evt.CurrentHP);
        }

        private void OnBlockGained(BlockGainedEvent evt)
        {
            if (evt.IsPlayer)
                UpdateBlock(evt.TotalBlock);
        }

        private void OnSpinsChanged(SpinsChangedEvent evt)
        {
            UpdateSpins(evt.SpinsRemaining);
        }

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            UpdateSpins(evt.SpinsAvailable);
            UpdateBlock(0);
            if (_turnText != null)
                _turnText.text = $"Turn {evt.TurnNumber}";
        }

        private void OnStatusApplied(StatusAppliedEvent evt)
        {
            if (evt.IsPlayer && _statusText != null)
                _statusText.text = $"{evt.StatusName} x{evt.Stacks}";
        }

        private void UpdateBlock(int block)
        {
            if (_blockText != null)
                _blockText.text = block > 0 ? $"Block: {block}" : "";
        }

        private void UpdateSpins(int spins)
        {
            if (_spinCountText != null)
                _spinCountText.text = $"Spins: {spins}";
        }
    }
}
