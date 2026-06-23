using SlotRogue.Core.Events;

namespace SlotRogue.Core.Battle
{
    public class TurnManager
    {
        public int TurnNumber { get; private set; }
        public int SpinsRemaining { get; private set; }
        public int BaseSpinsPerTurn { get; set; }
        public int BonusSpins { get; set; }

        public TurnManager(int baseSpins = 2)
        {
            BaseSpinsPerTurn = baseSpins;
            BonusSpins = 0;
            TurnNumber = 0;
        }

        public void StartNewTurn()
        {
            TurnNumber++;
            SpinsRemaining = BaseSpinsPerTurn + BonusSpins;

            EventBus.Publish(new TurnStartedEvent
            {
                TurnNumber = TurnNumber,
                SpinsAvailable = SpinsRemaining
            });
        }

        public bool ConsumeSpin()
        {
            if (SpinsRemaining <= 0) return false;
            SpinsRemaining--;

            EventBus.Publish(new SpinsChangedEvent
            {
                SpinsRemaining = SpinsRemaining
            });

            return true;
        }

        public void AddSpins(int count)
        {
            SpinsRemaining += count;

            EventBus.Publish(new SpinsChangedEvent
            {
                SpinsRemaining = SpinsRemaining
            });
        }

        public bool HasSpins => SpinsRemaining > 0;

        public void EndTurn()
        {
            EventBus.Publish(new TurnEndedEvent { TurnNumber = TurnNumber });
        }
    }
}
