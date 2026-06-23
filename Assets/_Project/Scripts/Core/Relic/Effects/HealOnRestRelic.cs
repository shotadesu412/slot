using SlotRogue.Core.Events;
using SlotRogue.Core.Map;

namespace SlotRogue.Core.Relic.Effects
{
    public class HealOnRestRelic : RelicEffect
    {
        private readonly int _bonusHeal;

        public HealOnRestRelic(int bonusHeal)
        {
            _bonusHeal = bonusHeal;
        }

        public override void OnAcquired(RunState run)
        {
            EventBus.Subscribe<RestChoiceEvent>(OnRest);
        }

        public override void OnRemoved(RunState run)
        {
            EventBus.Unsubscribe<RestChoiceEvent>(OnRest);
        }

        private void OnRest(RestChoiceEvent evt)
        {
            if (evt.ChoseHeal)
            {
                // Bonus heal is handled by checking this relic in the rest scene
            }
        }

        public int GetBonusHeal() => _bonusHeal;
    }
}
