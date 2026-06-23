namespace SlotRogue.Core.Battle
{
    public static class DamageCalculator
    {
        public static int CalculateAttackDamage(CombatUnit attacker, int baseDamage)
        {
            int bonus = attacker.GetStrengthBonus();
            return System.Math.Max(0, baseDamage + bonus);
        }

        public static int CalculateReceivedDamage(CombatUnit target, int incomingDamage)
        {
            float multiplier = 1f;
            if (target.HasStatus(StatusType.Vulnerable))
                multiplier = 1.5f;

            return (int)(incomingDamage * multiplier);
        }
    }
}
