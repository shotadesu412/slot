using UnityEngine;

namespace SlotRogue.Data
{
    public enum RelicRarity
    {
        Common,
        Uncommon,
        Rare,
        Boss
    }

    [CreateAssetMenu(menuName = "SlotRogue/Relic")]
    public class RelicData : ScriptableObject
    {
        public string relicId;
        public string relicName;
        [TextArea] public string description;
        public Sprite icon;
        public RelicRarity rarity;
        public string effectId;
        public int effectValue;
    }
}
