using SlotRogue.Core.Events;

namespace SlotRogue.Core.Skill.Effects
{
    public class DealDamageEffect : SkillEffect
    {
        public override void Execute(BattleContext context)
        {
            int damage = context.EffectValue;
            int actualDamage = context.Enemy.TakeDamage(damage);

            EventBus.Publish(new DamageDealtEvent
            {
                IsPlayer = false,
                Amount = actualDamage,
                RemainingHP = context.Enemy.CurrentHP
            });

            EventBus.Publish(new EffectResolvedEvent
            {
                SkillName = "Attack",
                Description = $"{actualDamage}ダメージを与えた"
            });
        }

        public override string GetDescription(int value) => $"{value}ダメージを与える";
    }
}
