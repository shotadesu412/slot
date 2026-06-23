using System.Collections.Generic;
using System.Linq;

namespace SlotRogue.Core.Battle
{
    public class CombatUnit
    {
        public string Name { get; set; }
        public int MaxHP { get; set; }
        public int CurrentHP { get; set; }
        public int Block { get; set; }
        public bool IsAlive => CurrentHP > 0;

        private readonly List<StatusEffect> _statuses = new();
        public IReadOnlyList<StatusEffect> Statuses => _statuses;

        public CombatUnit(string name, int maxHP)
        {
            Name = name;
            MaxHP = maxHP;
            CurrentHP = maxHP;
            Block = 0;
        }

        public int TakeDamage(int rawDamage)
        {
            float multiplier = 1f;
            if (HasStatus(StatusType.Vulnerable))
                multiplier = 1.5f;

            int damage = (int)(rawDamage * multiplier);

            int blockedDamage = System.Math.Min(Block, damage);
            Block -= blockedDamage;
            int hpDamage = damage - blockedDamage;

            CurrentHP = System.Math.Max(0, CurrentHP - hpDamage);
            return hpDamage;
        }

        public int TakeDirectDamage(int damage)
        {
            int actual = System.Math.Min(CurrentHP, damage);
            CurrentHP -= actual;
            return actual;
        }

        public void GainBlock(int amount)
        {
            Block += amount;
        }

        public void Heal(int amount)
        {
            CurrentHP = System.Math.Min(MaxHP, CurrentHP + amount);
        }

        public void ResetBlock()
        {
            Block = 0;
        }

        public void ApplyStatus(StatusType type, int stacks)
        {
            var existing = _statuses.FirstOrDefault(s => s.Type == type);
            if (existing != null)
                existing.Stacks += stacks;
            else
                _statuses.Add(new StatusEffect(type, stacks));
        }

        public bool HasStatus(StatusType type)
        {
            return _statuses.Any(s => s.Type == type && !s.IsExpired);
        }

        public int GetStatusStacks(StatusType type)
        {
            return _statuses.FirstOrDefault(s => s.Type == type)?.Stacks ?? 0;
        }

        public int GetStrengthBonus()
        {
            int bonus = GetStatusStacks(StatusType.Strength);
            if (HasStatus(StatusType.Weakness))
                bonus -= GetStatusStacks(StatusType.Weakness);
            return bonus;
        }

        public void TickStatuses()
        {
            foreach (var status in _statuses)
                status.TickEndOfTurn();

            _statuses.RemoveAll(s => s.IsExpired);
        }

        public int ProcessPoison()
        {
            var poison = _statuses.FirstOrDefault(s => s.Type == StatusType.Poison);
            if (poison == null || poison.IsExpired) return 0;

            int damage = poison.Stacks;
            CurrentHP = System.Math.Max(0, CurrentHP - damage);
            return damage;
        }

        public int ProcessRegeneration()
        {
            var regen = _statuses.FirstOrDefault(s => s.Type == StatusType.Regeneration);
            if (regen == null || regen.IsExpired) return 0;

            int heal = regen.Stacks;
            Heal(heal);
            return heal;
        }
    }
}
