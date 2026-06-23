using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SlotRogue.Core.Events;
using SlotRogue.Unity.Common;
using SlotRogue.Data;

namespace SlotRogue.Unity.Battle
{
    public class EnemyDisplay : MonoBehaviour
    {
        [SerializeField] private Image _spriteImage;
        [SerializeField] private HPBar _hpBar;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _intentText;
        [SerializeField] private Image _intentIcon;
        [SerializeField] private TextMeshProUGUI _blockText;

        public void Initialize(EnemyData data)
        {
            if (_nameText != null)
                _nameText.text = data.enemyName;
            if (_hpBar != null)
                _hpBar.Initialize(data.maxHP);
            if (_spriteImage != null && data.sprite != null)
                _spriteImage.sprite = data.sprite;

            EventBus.Subscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Subscribe<BlockGainedEvent>(OnBlockGained);
            EventBus.Subscribe<EnemyIntentRevealedEvent>(OnIntentRevealed);
            EventBus.Subscribe<EnemyActedEvent>(OnEnemyActed);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Unsubscribe<BlockGainedEvent>(OnBlockGained);
            EventBus.Unsubscribe<EnemyIntentRevealedEvent>(OnIntentRevealed);
            EventBus.Unsubscribe<EnemyActedEvent>(OnEnemyActed);
        }

        private void OnDamageDealt(DamageDealtEvent evt)
        {
            if (!evt.IsPlayer && _hpBar != null)
                _hpBar.SetHP(evt.RemainingHP);
        }

        private void OnBlockGained(BlockGainedEvent evt)
        {
            if (!evt.IsPlayer && _blockText != null)
                _blockText.text = evt.TotalBlock > 0 ? $"Block: {evt.TotalBlock}" : "";
        }

        private void OnIntentRevealed(EnemyIntentRevealedEvent evt)
        {
            if (_intentText != null)
            {
                string intentDisplay = evt.IntentType switch
                {
                    "Attack" => $"Attack {evt.Value}",
                    "Defend" => $"Defend {evt.Value}",
                    "Buff" => "Buff",
                    "Debuff" => "Debuff",
                    _ => "???"
                };
                _intentText.text = intentDisplay;
            }
        }

        private void OnEnemyActed(EnemyActedEvent evt)
        {
            if (_intentText != null)
                _intentText.text = "";
        }
    }
}
