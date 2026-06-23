using SlotRogue.Core.Events;

namespace SlotRogue.Core.Skill.Effects
{
    public class HealEffect : SkillEffect
    {
        public override void Execute(BattleContext context)
        {
            context.Player.Heal(context.EffectValue);

            EventBus.Publish(new HealEvent
            {
                IsPlayer = true,
                Amount = context.EffectValue,
                CurrentHP = context.Player.CurrentHP
            });

            EventBus.Publish(new EffectResolvedEvent
            {
                SkillName = "Heal",
                Description = $"HP{context.EffectValue}回復"
            });
        }

        public override string GetDescription(int value) => $"HP{value}回復";
    }
}
