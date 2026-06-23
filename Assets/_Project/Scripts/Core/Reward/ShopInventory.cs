using System;
using System.Collections.Generic;
using SlotRogue.Data;

namespace SlotRogue.Core.Reward
{
    public class ShopItem
    {
        public SkillData Skill { get; set; }
        public RelicData Relic { get; set; }
        public int Cost { get; set; }
        public bool IsSold { get; set; }

        public bool IsSkill => Skill != null;
        public bool IsRelic => Relic != null;
        public string Name => IsSkill ? Skill.skillName : (IsRelic ? Relic.relicName : "???");
    }

    public class ShopInventory
    {
        private readonly Random _rng;

        public List<ShopItem> Items { get; } = new();

        public ShopInventory(Random rng)
        {
            _rng = rng;
        }

        public void GenerateShop(List<SkillData> skillPool, List<RelicData> relicPool, int floor)
        {
            Items.Clear();

            // 3 skills
            var shuffledSkills = new List<SkillData>(skillPool);
            Shuffle(shuffledSkills);
            for (int i = 0; i < Math.Min(3, shuffledSkills.Count); i++)
            {
                Items.Add(new ShopItem
                {
                    Skill = shuffledSkills[i],
                    Cost = GetSkillCost(shuffledSkills[i])
                });
            }

            // 1-2 relics
            var shuffledRelics = new List<RelicData>(relicPool);
            Shuffle(shuffledRelics);
            int relicCount = Math.Min(2, shuffledRelics.Count);
            for (int i = 0; i < relicCount; i++)
            {
                Items.Add(new ShopItem
                {
                    Relic = shuffledRelics[i],
                    Cost = GetRelicCost(shuffledRelics[i])
                });
            }
        }

        private int GetSkillCost(SkillData skill)
        {
            return skill.rarity switch
            {
                SkillRarity.Common => 50 + _rng.Next(20),
                SkillRarity.Uncommon => 80 + _rng.Next(30),
                SkillRarity.Rare => 150 + _rng.Next(50),
                _ => 75
            };
        }

        private int GetRelicCost(RelicData relic)
        {
            return relic.rarity switch
            {
                RelicRarity.Common => 100 + _rng.Next(30),
                RelicRarity.Uncommon => 150 + _rng.Next(50),
                RelicRarity.Rare => 250 + _rng.Next(80),
                _ => 150
            };
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
