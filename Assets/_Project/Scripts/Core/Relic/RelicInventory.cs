using System.Collections.Generic;
using SlotRogue.Core.Map;
using SlotRogue.Data;

namespace SlotRogue.Core.Relic
{
    public class RelicInventory
    {
        private readonly List<RelicData> _relics = new();
        private readonly List<RelicEffect> _activeEffects = new();
        private readonly RunState _runState;

        public IReadOnlyList<RelicData> Relics => _relics;

        public RelicInventory(RunState runState)
        {
            _runState = runState;
        }

        public void AddRelic(RelicData relic)
        {
            _relics.Add(relic);
            _runState.Relics.Add(relic);

            var effect = RelicEffectFactory.Create(relic.effectId, relic.effectValue);
            if (effect != null)
            {
                effect.RelicId = relic.relicId;
                _activeEffects.Add(effect);
                effect.OnAcquired(_runState);
            }
        }

        public void RemoveRelic(string relicId)
        {
            var relic = _relics.Find(r => r.relicId == relicId);
            if (relic == null) return;

            _relics.Remove(relic);
            _runState.Relics.Remove(relic);

            var effect = _activeEffects.Find(e => e.RelicId == relicId);
            if (effect != null)
            {
                effect.OnRemoved(_runState);
                _activeEffects.Remove(effect);
            }
        }

        public bool HasRelic(string relicId)
        {
            return _relics.Exists(r => r.relicId == relicId);
        }
    }
}
