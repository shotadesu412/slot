using UnityEngine;

namespace SlotRogue.Data
{
    public enum CharacterMechanic
    {
        AimGauge,
        Berserk
    }

    [CreateAssetMenu(menuName = "SlotRogue/Character")]
    public class CharacterData : ScriptableObject
    {
        public string characterId;
        public string characterName;
        [TextArea] public string description;
        public Sprite portrait;
        public int startingHP = 80;
        public int baseSpinsPerTurn = 2;
        public CharacterMechanic mechanic;

        [Header("Starting Reels")]
        public SkillData[] startingReel1;
        public SkillData[] startingReel2;
        public SkillData[] startingReel3;
    }
}
