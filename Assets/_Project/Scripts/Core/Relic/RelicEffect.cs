using SlotRogue.Core.Map;

namespace SlotRogue.Core.Relic
{
    public abstract class RelicEffect
    {
        public string RelicId { get; set; }
        public abstract void OnAcquired(RunState run);
        public abstract void OnRemoved(RunState run);
    }
}
