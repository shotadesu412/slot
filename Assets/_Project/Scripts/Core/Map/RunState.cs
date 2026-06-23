using System;
using System.Collections.Generic;
using SlotRogue.Data;

namespace SlotRogue.Core.Map
{
    public class RunState
    {
        public CharacterData Character { get; set; }
        public int CurrentHP { get; set; }
        public int MaxHP { get; set; }
        public int Gold { get; set; }
        public int CurrentFloor { get; set; }
        public int? CurrentNodeId { get; set; }
        public int Seed { get; }

        public List<SkillData>[] ReelContents { get; }
        public List<RelicData> Relics { get; } = new();
        public List<MapNode> MapNodes { get; set; }
        public HashSet<int> VisitedNodeIds { get; } = new();

        private readonly Random _rng;

        public RunState(int seed = -1)
        {
            Seed = seed >= 0 ? seed : new Random().Next();
            _rng = new Random(Seed);

            ReelContents = new List<SkillData>[3];
            for (int i = 0; i < 3; i++)
                ReelContents[i] = new List<SkillData>();
        }

        public void InitializeFromCharacter(CharacterData character)
        {
            Character = character;
            MaxHP = character.startingHP;
            CurrentHP = MaxHP;
            Gold = 100;
            CurrentFloor = 0;

            ReelContents[0].Clear();
            ReelContents[1].Clear();
            ReelContents[2].Clear();

            if (character.startingReel1 != null)
                ReelContents[0].AddRange(character.startingReel1);
            if (character.startingReel2 != null)
                ReelContents[1].AddRange(character.startingReel2);
            if (character.startingReel3 != null)
                ReelContents[2].AddRange(character.startingReel3);
        }

        public void AddSkillToReel(int reelIndex, SkillData skill)
        {
            if (reelIndex >= 0 && reelIndex < 3)
                ReelContents[reelIndex].Add(skill);
        }

        public void VisitNode(int nodeId)
        {
            VisitedNodeIds.Add(nodeId);
            CurrentNodeId = nodeId;

            // Unlock connected nodes
            var node = MapNodes?.Find(n => n.Id == nodeId);
            if (node != null)
            {
                node.IsVisited = true;
                foreach (var connectedId in node.ConnectedNodeIds)
                {
                    var connected = MapNodes.Find(n => n.Id == connectedId);
                    if (connected != null)
                        connected.IsAccessible = true;
                }
            }
        }

        public Random GetRNG() => _rng;
    }
}
