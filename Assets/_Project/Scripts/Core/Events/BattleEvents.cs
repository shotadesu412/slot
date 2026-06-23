namespace SlotRogue.Core.Events
{
    public struct TurnStartedEvent
    {
        public int TurnNumber;
        public int SpinsAvailable;
    }

    public struct TurnEndedEvent
    {
        public int TurnNumber;
    }

    public struct SpinStartedEvent { }

    public struct ReelStoppedEvent
    {
        public int ReelIndex;
        public string SkillId;
        public string SkillName;
    }

    public struct AllReelsStoppedEvent
    {
        public string[] SkillIds;
    }

    public struct EffectResolvedEvent
    {
        public string SkillName;
        public string Description;
    }

    public struct DamageDealtEvent
    {
        public bool IsPlayer;
        public int Amount;
        public int RemainingHP;
    }

    public struct BlockGainedEvent
    {
        public bool IsPlayer;
        public int Amount;
        public int TotalBlock;
    }

    public struct HealEvent
    {
        public bool IsPlayer;
        public int Amount;
        public int CurrentHP;
    }

    public struct StatusAppliedEvent
    {
        public bool IsPlayer;
        public string StatusName;
        public int Stacks;
    }

    public struct SpinsChangedEvent
    {
        public int SpinsRemaining;
    }

    public struct EnemyIntentRevealedEvent
    {
        public string IntentType;
        public int Value;
    }

    public struct EnemyActedEvent
    {
        public string ActionDescription;
    }

    public struct BattleWonEvent { }

    public struct BattleLostEvent { }

    public struct BattleStartedEvent
    {
        public string EnemyName;
        public int EnemyHP;
        public int PlayerHP;
    }
}
