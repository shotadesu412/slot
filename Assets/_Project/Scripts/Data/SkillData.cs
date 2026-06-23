using UnityEngine;

namespace SlotRogue.Data
{
    public enum SkillRarity
    {
        Common,
        Uncommon,
        Rare
    }

    [CreateAssetMenu(menuName = "SlotRogue/Skill")]
    public class SkillData : ScriptableObject
    {
        public string skillId;
        public string skillName;
        [TextArea] public string description;
        public Sprite icon;
        public SkillRarity rarity = SkillRarity.Common;
        public string effectId;
        public int effectValue;
        public int upgradedValue;
        public bool isUpgraded;

        public int CurrentValue => isUpgraded ? upgradedValue : effectValue;
    }
}
