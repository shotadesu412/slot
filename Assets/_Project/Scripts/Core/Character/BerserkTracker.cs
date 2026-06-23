using SlotRogue.Core.Events;

namespace SlotRogue.Core.Character
{
    public struct BerserkDamageMultiplierChangedEvent { public float Multiplier; }

    public class BerserkTracker
    {
        public int TotalSelfDamage { get; private set; }
        public float MultiplierPerDamage { get; set; } = 0.05f;
        public float BaseMultiplier { get; set; } = 1.0f;

        public float DamageMultiplier => BaseMultiplier + (TotalSelfDamage * MultiplierPerDamage);

        public void OnSelfDamageDealt(int amount)
        {
            TotalSelfDamage += amount;

            EventBus.Publish(new BerserkDamageMultiplierChangedEvent
            {
                Multiplier = DamageMultiplier
            });
        }

        public int ApplyMultiplier(int baseDamage)
        {
            return (int)(baseDamage * DamageMultiplier);
        }

        public void ResetForBattle()
        {
            TotalSelfDamage = 0;
        }
    }
}
