using SlotRogue.Core.Map;
using SlotRogue.Unity.Bootstrap;

namespace SlotRogue.Core.Relic.Effects
{
    public class ExtraReelSlotRelic : RelicEffect
    {
        private readonly int _extraSlots;

        public ExtraReelSlotRelic(int extraSlots)
        {
            _extraSlots = extraSlots;
        }

        public override void OnAcquired(RunState run)
        {
            // Increase max symbols on all reels
            var controller = ServiceLocator.Get<Battle.BattleController>();
            if (controller != null)
            {
                foreach (var reel in controller.SlotMachine.Reels)
                    reel.MaxSymbols += _extraSlots;
            }
        }

        public override void OnRemoved(RunState run)
        {
            var controller = ServiceLocator.Get<Battle.BattleController>();
            if (controller != null)
            {
                foreach (var reel in controller.SlotMachine.Reels)
                    reel.MaxSymbols -= _extraSlots;
            }
        }
    }
}
