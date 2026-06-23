using SlotRogue.Core.Events;

namespace SlotRogue.Core.Skill.Effects
{
    public class DrawSpinEffect : SkillEffect
    {
        public override void Execute(BattleContext context)
        {
            context.Battle.TurnManager.AddSpins(context.EffectValue);

            EventBus.Publish(new EffectResolvedEvent
            {
                SkillName = "Draw Spin",
                Description = $"スピン{context.EffectValue}回追加！"
            });
        }

        public override string GetDescription(int value) => $"スピン{value}回追加";
    }
}
