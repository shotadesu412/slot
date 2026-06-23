using SlotRogue.Core.Events;
using SlotRogue.Core.Map;
using SlotRogue.Unity.Bootstrap;

namespace SlotRogue.Core.Relic.Effects
{
    public class ExtraSpinRelic : RelicEffect
    {
        private readonly int _extraSpins;

        public ExtraSpinRelic(int extraSpins)
        {
            _extraSpins = extraSpins;
        }

        public override void OnAcquired(RunState run)
        {
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
        }

        public override void OnRemoved(RunState run)
        {
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
        }

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            var controller = ServiceLocator.Get<Battle.BattleController>();
            controller?.TurnManager.AddSpins(_extraSpins);
        }
    }
}
