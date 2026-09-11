// Engine/Combat/DamageCalculator.cs
using OgreBattle.Engine.Entities;

namespace OgreBattle.Engine.Combat
{
    public class DamageCalculator
    {
        public int CalculateDamage(Unit attacker, Unit defender, Ability ability)
        {
            var baseStat = ability.IsMagic ? attacker.INT : attacker.STR;
            var defenseStat = ability.IsMagic ? defender.INT : defender.STR;

            var raw = baseStat + ability.Power - defenseStat;
            if (raw < 1) raw = 1;
            return raw;
        }
    }
}
