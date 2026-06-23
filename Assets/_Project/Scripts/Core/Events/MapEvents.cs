using SlotRogue.Core.Map;

namespace SlotRogue.Core.Events
{
    public struct NodeSelectedEvent
    {
        public int NodeId;
        public FloorType Type;
    }

    public struct FloorCompletedEvent
    {
        public int Floor;
        public FloorType Type;
    }

    public struct RunStartedEvent
    {
        public string CharacterName;
        public int Seed;
    }

    public struct RunEndedEvent
    {
        public bool Victory;
        public int FloorsCleared;
    }
}
