using System;
using System.Collections.Generic;
using SlotRogue.Core.Relic.Effects;

namespace SlotRogue.Core.Relic
{
    public static class RelicEffectFactory
    {
        private static readonly Dictionary<string, Func<int, RelicEffect>> _registry = new()
        {
            { "extra_spin", (v) => new ExtraSpinRelic(v) },
            { "extra_reel_slot", (v) => new ExtraReelSlotRelic(v) },
            { "heal_on_rest", (v) => new HealOnRestRelic(v) },
            { "starting_block", (v) => new StartingBlockRelic(v) },
        };

        public static RelicEffect Create(string effectId, int value)
        {
            if (_registry.TryGetValue(effectId, out var factory))
                return factory(value);
            return null;
        }

        public static void Register(string effectId, Func<int, RelicEffect> factory)
        {
            _registry[effectId] = factory;
        }
    }
}
