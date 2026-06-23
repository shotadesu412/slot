namespace SlotRogue.Core.Battle
{
    public enum StatusType
    {
        Strength,   // +damage per attack
        Weakness,   // -damage per attack
        Poison,     // damage per turn, decrements
        Regeneration, // heal per turn, decrements
        Vulnerable, // take more damage
    }

    public class StatusEffect
    {
        public StatusType Type { get; }
        public int Stacks { get; set; }

        public StatusEffect(StatusType type, int stacks)
        {
            Type = type;
            Stacks = stacks;
        }

        public void TickEndOfTurn()
        {
            switch (Type)
            {
                case StatusType.Poison:
                case StatusType.Regeneration:
                    Stacks--;
                    break;
                case StatusType.Weakness:
                case StatusType.Vulnerable:
                    Stacks--;
                    break;
            }
        }

        public bool IsExpired => Stacks <= 0;
    }
}
