using System;
using System.Collections.Generic;
using System.Linq;
using SlotRogue.Data;

namespace SlotRogue.Core.Reward
{
    public class RewardGenerator
    {
        private readonly List<SkillData> _skillPool;
        private readonly Random _rng;

        public RewardGenerator(List<SkillData> skillPool, Random rng)
        {
            _skillPool = skillPool;
            _rng = rng;
        }

        public List<SkillData> GenerateSkillRewards(int count = 3, int floor = 0)
        {
            var available = new List<SkillData>(_skillPool);
            var results = new List<SkillData>();

            // Higher floors have better rarity chance
            float rareChance = 0.05f + floor * 0.02f;
            float uncommonChance = 0.2f + floor * 0.02f;

            for (int i = 0; i < count && available.Count > 0; i++)
            {
                SkillData picked;
                float roll = (float)_rng.NextDouble();

                if (roll < rareChance)
                    picked = PickByRarity(available, SkillRarity.Rare);
                else if (roll < rareChance + uncommonChance)
                    picked = PickByRarity(available, SkillRarity.Uncommon);
                else
                    picked = PickByRarity(available, SkillRarity.Common);

                if (picked != null)
                {
                    results.Add(picked);
                    available.Remove(picked);
                }
            }

            return results;
        }

        private SkillData PickByRarity(List<SkillData> pool, SkillRarity targetRarity)
        {
            var filtered = pool.Where(s => s.rarity == targetRarity).ToList();
            if (filtered.Count == 0)
                filtered = pool;

            return filtered[_rng.Next(filtered.Count)];
        }

        public int GenerateGoldReward(int baseGold = 20, int floor = 0)
        {
            return baseGold + _rng.Next(0, 10) + floor * 2;
        }
    }
}
