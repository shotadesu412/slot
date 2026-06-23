using SlotRogue.Core.Events;
using SlotRogue.Data;

namespace SlotRogue.Core.Slot
{
    public class SlotMachine
    {
        public Reel[] Reels { get; }
        public bool IsSpinning { get; private set; }
        public int ReelCount => Reels.Length;

        private readonly SkillData[] _stoppedResults;
        private int _stoppedCount;

        public SlotMachine()
        {
            Reels = new Reel[3];
            for (int i = 0; i < 3; i++)
                Reels[i] = new Reel();

            _stoppedResults = new SkillData[3];
        }

        public void StartSpin()
        {
            _stoppedCount = 0;
            for (int i = 0; i < 3; i++)
                _stoppedResults[i] = null;

            IsSpinning = true;

            foreach (var reel in Reels)
                reel.StartSpin();

            EventBus.Publish(new SpinStartedEvent());
        }

        public void Update(float deltaTime)
        {
            if (!IsSpinning) return;

            foreach (var reel in Reels)
            {
                if (reel.IsSpinning)
                    reel.UpdateSpin(deltaTime);
            }
        }

        public SkillData StopReel(int index)
        {
            if (index < 0 || index >= 3) return null;
            if (!Reels[index].IsSpinning) return null;

            var result = Reels[index].Stop();
            _stoppedResults[index] = result;
            _stoppedCount++;

            EventBus.Publish(new ReelStoppedEvent
            {
                ReelIndex = index,
                SkillId = result?.skillId ?? "",
                SkillName = result?.skillName ?? "Empty"
            });

            if (_stoppedCount >= 3)
            {
                IsSpinning = false;
                EventBus.Publish(new AllReelsStoppedEvent
                {
                    SkillIds = new[]
                    {
                        _stoppedResults[0]?.skillId ?? "",
                        _stoppedResults[1]?.skillId ?? "",
                        _stoppedResults[2]?.skillId ?? ""
                    }
                });
            }

            return result;
        }

        public SpinResult GetSpinResult()
        {
            return new SpinResult(_stoppedResults[0], _stoppedResults[1], _stoppedResults[2]);
        }

        public bool IsReelSpinning(int index)
        {
            return index >= 0 && index < 3 && Reels[index].IsSpinning;
        }
    }
}
