namespace SlotRogue.Core.Battle
{
    public enum BattleState
    {
        NotStarted,
        PlayerTurnStart,
        WaitingForSpin,
        Spinning,
        WaitingForReelStop,
        ResolvingEffects,
        EnemyTurn,
        Victory,
        Defeat
    }
}
