namespace SlotRogue.Core.Battle
{
    public enum IntentType
    {
        Attack,
        Defend,
        Buff,
        Debuff
    }

    public class EnemyIntent
    {
        public IntentType Type { get; }
        public int Value { get; }

        public EnemyIntent(IntentType type, int value)
        {
            Type = type;
            Value = value;
        }
    }
}
