using System.Collections.Generic;
using SlotRogue.Data;

namespace SlotRogue.Core.Slot
{
    public class Reel
    {
        public List<SkillData> Symbols { get; private set; } = new();
        public int CurrentIndex { get; private set; }
        public bool IsSpinning { get; private set; }
        public float SpinSpeed { get; set; } = 1.0f;
        public int MaxSymbols { get; set; } = 8;

        private float _spinPosition;

        public void AddSymbol(SkillData skill)
        {
            if (Symbols.Count < MaxSymbols)
                Symbols.Add(skill);
        }

        public void RemoveSymbol(int index)
        {
            if (index >= 0 && index < Symbols.Count && Symbols.Count > 1)
                Symbols.RemoveAt(index);
        }

        public void StartSpin()
        {
            if (Symbols.Count == 0) return;
            IsSpinning = true;
            _spinPosition = CurrentIndex;
        }

        public void UpdateSpin(float deltaTime)
        {
            if (!IsSpinning || Symbols.Count == 0) return;

            _spinPosition += SpinSpeed * deltaTime * 10f;
            if (_spinPosition >= Symbols.Count)
                _spinPosition -= Symbols.Count;

            CurrentIndex = (int)_spinPosition % Symbols.Count;
        }

        public SkillData Stop()
        {
            IsSpinning = false;
            if (Symbols.Count == 0) return null;
            CurrentIndex = CurrentIndex % Symbols.Count;
            return Symbols[CurrentIndex];
        }

        public SkillData StopAtIndex(int index)
        {
            IsSpinning = false;
            if (Symbols.Count == 0) return null;
            CurrentIndex = index % Symbols.Count;
            return Symbols[CurrentIndex];
        }

        public SkillData GetCurrentSymbol()
        {
            if (Symbols.Count == 0) return null;
            return Symbols[CurrentIndex % Symbols.Count];
        }

        public SkillData GetSymbolAt(int offset)
        {
            if (Symbols.Count == 0) return null;
            int idx = ((CurrentIndex + offset) % Symbols.Count + Symbols.Count) % Symbols.Count;
            return Symbols[idx];
        }
    }
}
