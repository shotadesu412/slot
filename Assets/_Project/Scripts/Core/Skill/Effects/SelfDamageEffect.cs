using SlotRogue.Core.Events;

namespace SlotRogue.Core.Skill.Effects
{
    public class SelfDamageEffect : SkillEffect
    {
        public override void Execute(BattleContext context)
        {
            int selfDmg = context.EffectValue / 3;
            int actual = context.Player.TakeDirectDamage(selfDmg);

            EventBus.Publish(new DamageDealtEvent
            {
                IsPlayer = true,
                Amount = actual,
                RemainingHP = context.Player.CurrentHP
            });

            int attackDmg = context.EffectValue;
            int dealt = context.Enemy.TakeDamage(attackDmg);

            EventBus.Publish(new DamageDealtEvent
            {
                IsPlayer = false,
                Amount = dealt,
                RemainingHP = context.Enemy.CurrentHP
            });

            EventBus.Publish(new EffectResolvedEvent
            {
                SkillName = "Berserk Strike",
                Description = $"自傷{actual} → {dealt}ダメージ！"
            });
        }

        public override string GetDescription(int value) => $"自傷{value / 3} → {value}ダメージ";
    }
}
