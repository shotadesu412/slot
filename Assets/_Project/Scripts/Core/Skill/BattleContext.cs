using SlotRogue.Core.Battle;

namespace SlotRogue.Core.Skill
{
    public class BattleContext
    {
        public CombatUnit Player { get; set; }
        public CombatUnit Enemy { get; set; }
        public BattleController Battle { get; set; }
        public int EffectValue { get; set; }
        public int ReelIndex { get; set; }
    }
}
