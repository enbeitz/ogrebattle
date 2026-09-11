// Engine/Combat/BattleManager.cs
using System.Collections.Generic;
using OgreBattle.Engine.Entities;

namespace OgreBattle.Engine.Combat
{
    public class BattleManager
    {
        public List<Unit> ArmyA { get; }
        public List<Unit> ArmyB { get; }

        private readonly DamageCalculator _damageCalculator = new DamageCalculator();
        private readonly AIController _ai = new AIController();

        public bool IsFinished { get; private set; }

        public BattleManager(List<Unit> armyA, List<Unit> armyB)
        {
            ArmyA = armyA;
            ArmyB = armyB;
        }

        public void Step()
        {
            if (IsFinished) return;

            var attacker = _ai.ChooseAttacker(ArmyA);
            var target = _ai.ChooseTarget(ArmyB);

            if (attacker == null || target == null)
            {
                IsFinished = true;
                return;
            }

            var basicAttack = new Ability("basic_attack", "Attack", 10);
            var damage = _damageCalculator.CalculateDamage(attacker, target, basicAttack);

            target.HP -= damage;
            if (target.HP < 0) target.HP = 0;

            if (!AnyAlive(ArmyB) || !AnyAlive(ArmyA))
                IsFinished = true;
        }

        private bool AnyAlive(List<Unit> units)
        {
            foreach (var unit in units)
                if (unit.IsAlive)
                    return true;

            return false;
        }
    }
}
