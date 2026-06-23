using SlotRogue.Core.Events;
using SlotRogue.Core.Map;
using SlotRogue.Unity.Bootstrap;

namespace SlotRogue.Core.Relic.Effects
{
    public class StartingBlockRelic : RelicEffect
    {
        private readonly int _blockAmount;

        public StartingBlockRelic(int blockAmount)
        {
            _blockAmount = blockAmount;
        }

        public override void OnAcquired(RunState run)
        {
            EventBus.Subscribe<BattleStartedEvent>(OnBattleStarted);
        }

        public override void OnRemoved(RunState run)
        {
            EventBus.Unsubscribe<BattleStartedEvent>(OnBattleStarted);
        }

        private void OnBattleStarted(BattleStartedEvent evt)
        {
            var controller = ServiceLocator.Get<Battle.BattleController>();
            controller?.Player.GainBlock(_blockAmount);
        }
    }
}
