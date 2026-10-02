using HearthAndHavoc_GoblinLegacy.AI.AsyncProcessor;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using Microsoft.Xna.Framework;
using System;

using static HearthAndHavoc_GoblinLegacy.AI.AsyncProcessor.ProcessorThread;

namespace HearthAndHavoc_GoblinLegacy.AI.Action
{
    // Base for actions that ask the AI thread for an A* path (Milestone 7, decision 3).
    public abstract class PathingAction : BaseAction
    {
        private const float OrthogonalStepCost = 1f;
        private const float DiagonalStepCost = 1.4f;

        protected Nullable<JobHandle> JobHandle { get; set; }
        protected bool IsActionAwaitingJobHandle()
        {
            if (!JobHandle.HasValue) return false;

            if (GoblinGame.Processor.IsCompleted(JobHandle.Value))
            {
                JobHandle = null;
                return false;
            }
            else return true;
        }

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

        protected abstract void CalculatePath(WorldSnapshot ws, CreatureSnapshot cs);

        // The snapshots handed to the AI thread for a path job: the locale map and where the creature stands now.
        // Virtual so a future pathing action that needs more in its snapshot can override it.
        protected virtual (WorldSnapshot ws, CreatureSnapshot cs) GenerateSnapshots(Creature creature)
        {
            return (new WorldSnapshot(creature.Locale.LocaleMap), new CreatureSnapshot(creature.Position));
        }
    }
}
