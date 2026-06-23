using System;
using System.Collections.Generic;

namespace SlotRogue.Core.Map
{
    public class MapGenerator
    {
        private readonly Random _rng;
        private int _nextId;

        public MapGenerator(int seed)
        {
            _rng = new Random(seed);
        }

        public List<MapNode> Generate(int totalFloors = 15, int minWidth = 2, int maxWidth = 4)
        {
            var nodes = new List<MapNode>();
            var floorNodes = new List<List<MapNode>>();
            _nextId = 0;

            for (int floor = 0; floor < totalFloors; floor++)
            {
                var type = DetermineFloorType(floor, totalFloors);
                int width = (floor == 0 || floor == totalFloors - 1)
                    ? 1
                    : _rng.Next(minWidth, maxWidth + 1);

                var currentFloor = new List<MapNode>();

                if (floor == totalFloors - 1)
                {
                    // Boss floor: single node
                    var boss = new MapNode(_nextId++, floor, 0, FloorType.Boss);
                    currentFloor.Add(boss);
                    nodes.Add(boss);
                }
                else if (floor == 0)
                {
                    // Starting floor: single battle
                    var start = new MapNode(_nextId++, floor, 0, FloorType.Battle);
                    currentFloor.Add(start);
                    nodes.Add(start);
                }
                else
                {
                    for (int col = 0; col < width; col++)
                    {
                        FloorType nodeType = type;
                        // Add variety within floors
                        if (type == FloorType.Battle && _rng.NextDouble() < 0.2)
                            nodeType = FloorType.Event;

                        var node = new MapNode(_nextId++, floor, col, nodeType);
                        currentFloor.Add(node);
                        nodes.Add(node);
                    }
                }

                // Connect to previous floor
                if (floor > 0 && floorNodes.Count > 0)
                {
                    var prevFloor = floorNodes[floor - 1];
                    ConnectFloors(prevFloor, currentFloor);
                }

                floorNodes.Add(currentFloor);
            }

            // Mark first floor as accessible
            if (floorNodes.Count > 0)
                foreach (var node in floorNodes[0])
                    node.IsAccessible = true;

            return nodes;
        }

        private void ConnectFloors(List<MapNode> prevFloor, List<MapNode> currentFloor)
        {
            // Every node must have at least one connection forward
            foreach (var prev in prevFloor)
            {
                if (currentFloor.Count == 1)
                {
                    prev.ConnectedNodeIds.Add(currentFloor[0].Id);
                    continue;
                }

                // Connect to closest column, plus random extra
                int closestCol = Math.Min(prev.Column, currentFloor.Count - 1);
                prev.ConnectedNodeIds.Add(currentFloor[closestCol].Id);

                // Chance to connect to adjacent column
                if (closestCol + 1 < currentFloor.Count && _rng.NextDouble() < 0.4)
                    prev.ConnectedNodeIds.Add(currentFloor[closestCol + 1].Id);
                if (closestCol - 1 >= 0 && _rng.NextDouble() < 0.4)
                    prev.ConnectedNodeIds.Add(currentFloor[closestCol - 1].Id);
            }

            // Ensure every current floor node has at least one incoming connection
            foreach (var current in currentFloor)
            {
                bool hasIncoming = false;
                foreach (var prev in prevFloor)
                {
                    if (prev.ConnectedNodeIds.Contains(current.Id))
                    {
                        hasIncoming = true;
                        break;
                    }
                }

                if (!hasIncoming)
                {
                    // Connect from random previous node
                    var randomPrev = prevFloor[_rng.Next(prevFloor.Count)];
                    randomPrev.ConnectedNodeIds.Add(current.Id);
                }
            }
        }

        private FloorType DetermineFloorType(int floor, int totalFloors)
        {
            if (floor == totalFloors - 1) return FloorType.Boss;
            if (floor == 0) return FloorType.Battle;

            // Fixed shop/rest floors
            if (floor == 4 || floor == 9) return FloorType.Shop;
            if (floor == 6 || floor == 12) return FloorType.Rest;

            // Elite floors
            if (floor == 7 || floor == 11) return FloorType.Elite;

            return FloorType.Battle;
        }
    }
}
