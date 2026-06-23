using SlotRogue.Core.Events;

namespace SlotRogue.Core.Skill.Effects
{
    public class GainBlockEffect : SkillEffect
    {
        public override void Execute(BattleContext context)
        {
            context.Player.GainBlock(context.EffectValue);

            EventBus.Publish(new BlockGainedEvent
            {
                IsPlayer = true,
                Amount = context.EffectValue,
                TotalBlock = context.Player.Block
            });

            EventBus.Publish(new EffectResolvedEvent
            {
                SkillName = "Block",
                Description = $"ブロック{context.EffectValue}を獲得"
            });
        }

        public override string GetDescription(int value) => $"ブロック{value}を得る";
    }
}
