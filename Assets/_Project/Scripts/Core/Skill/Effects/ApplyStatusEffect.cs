using SlotRogue.Core.Battle;
using SlotRogue.Core.Events;

namespace SlotRogue.Core.Skill.Effects
{
    public class ApplyPoisonEffect : SkillEffect
    {
        public override void Execute(BattleContext context)
        {
            context.Enemy.ApplyStatus(StatusType.Poison, context.EffectValue);

            EventBus.Publish(new StatusAppliedEvent
            {
                IsPlayer = false,
                StatusName = "Poison",
                Stacks = context.EffectValue
            });

            EventBus.Publish(new EffectResolvedEvent
            {
                SkillName = "Poison",
                Description = $"毒{context.EffectValue}を付与"
            });
        }

        public override string GetDescription(int value) => $"毒{value}を付与";
    }

    public class ApplyVulnerableEffect : SkillEffect
    {
        public override void Execute(BattleContext context)
        {
            context.Enemy.ApplyStatus(StatusType.Vulnerable, context.EffectValue);

            EventBus.Publish(new StatusAppliedEvent
            {
                IsPlayer = false,
                StatusName = "Vulnerable",
                Stacks = context.EffectValue
            });

            EventBus.Publish(new EffectResolvedEvent
            {
                SkillName = "Expose",
                Description = $"脆弱{context.EffectValue}を付与"
            });
        }

        public override string GetDescription(int value) => $"脆弱{value}を付与";
    }

    public class ApplyStrengthEffect : SkillEffect
    {
        public override void Execute(BattleContext context)
        {
            context.Player.ApplyStatus(StatusType.Strength, context.EffectValue);

            EventBus.Publish(new StatusAppliedEvent
            {
                IsPlayer = true,
                StatusName = "Strength",
                Stacks = context.EffectValue
            });

            EventBus.Publish(new EffectResolvedEvent
            {
                SkillName = "Power Up",
                Description = $"筋力{context.EffectValue}アップ"
            });
        }

        public override string GetDescription(int value) => $"筋力{value}アップ";
    }
}
