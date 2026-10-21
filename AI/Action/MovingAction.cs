using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HearthAndHavoc_GoblinLegacy.AI.Action
{
    // Base for actions that walk a creature along a path of tiles. It owns the movement budget and the stepping
    // loop (Milestone 6, decision 16), so every moving action pays for steps the same way; what to do when a step
    // is blocked or the path runs out stays with each action.
    public abstract class MovingAction : BaseAction
    {
        private const float OrthogonalStepCost = 1f;
        private const float DiagonalStepCost = 1.4f;

        private float _budget = 0f;

        // What it costs to step from the creature's tile to next: 1 orthogonally or 1.4 diagonally (the 5:7 ratio
        // A* uses), times the entered tile's move cost.
        protected static float GetStepCost(Creature creature, Point next)
        {
            Point current = creature.Position;
            if (next == current) return 0f;

            bool diagonal = next.X != current.X && next.Y != current.Y;
            float baseCost = diagonal ? DiagonalStepCost : OrthogonalStepCost;
            return baseCost * creature.Locale.LocaleMap.GetMoveCost(next);
        }

        // Sets the creature's activity to mode, adds this tick's movement for that mode to the budget, then takes
        // steps from the top of path while the budget covers the next one. Leftover budget carries over to the next
        // tick. A blocked step resets the budget to 0, so a waiting creature can't save up movement. stopAfterStep,
        // when given, is checked after each step taken and ends the stepping early when it returns true.
        protected StepResult StepAlong(Creature creature, Stack<Point> path, CreatureActivity mode, [AllowNull] Func<Creature, bool> stopAfterStep = null)
        {
            creature.Activity = mode;
            _budget += creature.GetMoveSpeed(mode);

            while (path.Count > 0)
            {
                Point next = path.Peek();
                float cost = GetStepCost(creature, next);
                if (_budget < cost) return StepResult.OUT_OF_BUDGET;

                if (!creature.Locale.MoveCreature(creature, next))
                {
                    _budget = 0f;
                    return StepResult.BLOCKED;
                }

                _budget -= cost;
                path.Pop();
                if (stopAfterStep != null && stopAfterStep(creature)) return StepResult.STOPPED;
            }

            return StepResult.PATH_DONE;
        }
    }
}
