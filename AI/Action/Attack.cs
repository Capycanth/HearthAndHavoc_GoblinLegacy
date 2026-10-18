using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using HearthAndHavoc_GoblinLegacy.Utility.Map;
using System;
using System.Diagnostics.CodeAnalysis;

namespace HearthAndHavoc_GoblinLegacy.AI.Action
{
    // Strikes a creature standing next to the attacker with the attacker's weapon (Milestone 7, decision 24).
    // Each strike deals the weapon's Damage scaled by attacker weight over target weight. Ends with TARGET_DEAD
    // when the target is dead, whether this attack killed it or not, keeping its carcass in Carcass for the chain
    // to eat, or TARGET_ESCAPED when the target gets out of reach.
    public class Attack : BaseAction
    {
        private const int Reach = 1;

        private readonly Creature _target;
        private float _attackBudget = 0f;

        // The target's carcass once it is dead, whatever killed it; null while it is alive.
        [AllowNull]
        public Carcass Carcass { get; private set; } = null;

        public Attack(Creature target)
        {
            _target = target;
        }

        public override ActionOutcome Perform(Creature creature)
        {
            WeaponDef weapon = creature.Weapon;
            if (weapon == null)
            {
                throw new InvalidOperationException($"Creature {creature.Id} has no weapon to attack with.");
            }

            // Dead before this strike (another attacker, starvation): its carcass is still there to eat.
            if (_target.IsDead)
            {
                Carcass = _target.Remains;
                return Finish(creature, ActionOutcome.TARGET_DEAD);
            }
            if (MapUtil.GetRingDistance(creature.Position, _target.Position) > Reach) return Finish(creature, ActionOutcome.TARGET_ESCAPED);

            creature.Activity = CreatureActivity.ATTACKING;

            // Like movement, attacks accumulate a budget each tick, so 6 per minute means 6 strikes this tick and
            // 0.5 per minute means one strike every other tick.
            _attackBudget += weapon.AttacksPerMinute;
            while (_attackBudget >= 1f)
            {
                _attackBudget -= 1f;
                _target.TakeDamage(weapon.Damage * creature.WeightKg / _target.WeightKg);

                if (_target.IsDead)
                {
                    Carcass = _target.Remains;
                    return Finish(creature, ActionOutcome.TARGET_DEAD);
                }
            }

            return ActionOutcome.RUNNING;
        }

        private static ActionOutcome Finish(Creature creature, ActionOutcome outcome)
        {
            creature.Activity = CreatureActivity.RESTING;
            return outcome;
        }
    }
}
