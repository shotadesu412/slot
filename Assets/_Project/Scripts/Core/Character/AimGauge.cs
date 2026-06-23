using SlotRogue.Core.Events;

namespace SlotRogue.Core.Character
{
    public struct AimGaugeReadyEvent { }
    public struct AimGaugeUsedEvent { }
    public struct AimGaugeChangedEvent { public float FillAmount; }
    public struct OverclockActivatedEvent { public float SpeedMultiplier; public int StrengthGain; }

    public class AimGauge
    {
        public float FillAmount { get; private set; }
        public float MaxFill { get; set; } = 100f;
        public float FillPerSpin { get; set; } = 25f;
        public bool IsReady => FillAmount >= MaxFill;
        public bool IsActive { get; private set; }

        public float SlowdownMultiplier { get; set; } = 0.2f;
        public float SlowdownDuration { get; set; } = 1.5f;

        private float _overclockSpeedBonus;

        public void OnSpinCompleted()
        {
            if (IsActive) return;

            FillAmount = System.Math.Min(MaxFill, FillAmount + FillPerSpin);

            EventBus.Publish(new AimGaugeChangedEvent { FillAmount = FillAmount / MaxFill });

            if (IsReady)
                EventBus.Publish(new AimGaugeReadyEvent());
        }

        public float ActivateAim()
        {
            if (!IsReady) return 1f;

            IsActive = true;
            FillAmount = 0;

            EventBus.Publish(new AimGaugeUsedEvent());
            EventBus.Publish(new AimGaugeChangedEvent { FillAmount = 0 });

            return SlowdownMultiplier;
        }

        public void DeactivateAim()
        {
            IsActive = false;
        }

        public float GetCurrentSpeedMultiplier()
        {
            float base_speed = 1f + _overclockSpeedBonus;
            if (IsActive)
                return base_speed * SlowdownMultiplier;
            return base_speed;
        }

        public int ActivateOverclock(float speedBonus = 0.5f, int strengthGain = 2)
        {
            _overclockSpeedBonus += speedBonus;

            EventBus.Publish(new OverclockActivatedEvent
            {
                SpeedMultiplier = 1f + _overclockSpeedBonus,
                StrengthGain = strengthGain
            });

            return strengthGain;
        }

        public void Reset()
        {
            FillAmount = 0;
            IsActive = false;
            _overclockSpeedBonus = 0;
        }
    }
}
