using System;
using System.Collections.Generic;
using SlotRogue.Core.Skill.Effects;

namespace SlotRogue.Core.Skill
{
    public static class SkillEffectFactory
    {
        private static readonly Dictionary<string, Func<SkillEffect>> _registry = new()
        {
            { "deal_damage", () => new DealDamageEffect() },
            { "gain_block", () => new GainBlockEffect() },
            { "self_damage", () => new SelfDamageEffect() },
            { "heal", () => new HealEffect() },
            { "draw_spin", () => new DrawSpinEffect() },
            { "apply_poison", () => new ApplyPoisonEffect() },
            { "apply_vulnerable", () => new ApplyVulnerableEffect() },
            { "apply_strength", () => new ApplyStrengthEffect() },
        };

        public static SkillEffect Create(string effectId)
        {
            if (_registry.TryGetValue(effectId, out var factory))
                return factory();

            throw new ArgumentException($"Unknown effect ID: {effectId}");
        }

        public static void Register(string effectId, Func<SkillEffect> factory)
        {
            _registry[effectId] = factory;
        }
    }
}
