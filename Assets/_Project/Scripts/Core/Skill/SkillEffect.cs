namespace SlotRogue.Core.Skill
{
    public abstract class SkillEffect
    {
        public abstract void Execute(BattleContext context);
        public virtual string GetDescription(int value) => "";
    }
}
