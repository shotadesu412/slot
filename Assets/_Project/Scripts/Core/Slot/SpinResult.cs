using SlotRogue.Data;

namespace SlotRogue.Core.Slot
{
    public class SpinResult
    {
        public SkillData[] Results { get; }

        public SpinResult(SkillData reel1, SkillData reel2, SkillData reel3)
        {
            Results = new[] { reel1, reel2, reel3 };
        }

        public SkillData this[int index] => Results[index];
    }
}
