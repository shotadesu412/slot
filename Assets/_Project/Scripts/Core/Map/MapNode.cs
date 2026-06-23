using System.Collections.Generic;

namespace SlotRogue.Core.Map
{
    public class MapNode
    {
        public int Id { get; }
        public int Floor { get; }
        public int Column { get; }
        public FloorType Type { get; set; }
        public bool IsVisited { get; set; }
        public bool IsAccessible { get; set; }
        public List<int> ConnectedNodeIds { get; } = new();

        public MapNode(int id, int floor, int column, FloorType type)
        {
            Id = id;
            Floor = floor;
            Column = column;
            Type = type;
        }
    }
}
