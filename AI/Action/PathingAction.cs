using HearthAndHavoc_GoblinLegacy.AI.AsyncProcessor;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using System;

using static HearthAndHavoc_GoblinLegacy.AI.AsyncProcessor.ProcessorThread;

namespace HearthAndHavoc_GoblinLegacy.AI.Action
{
    // Base for moving actions that ask the AI thread for an A* path (Milestone 7, decision 3).
    public abstract class PathingAction : MovingAction
    {
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

        protected abstract void CalculatePath(WorldSnapshot ws, CreatureSnapshot cs);

        // The snapshots handed to the AI thread for a path job: the locale map and where the creature stands now.
        // Virtual so a future pathing action that needs more in its snapshot can override it.
        protected virtual (WorldSnapshot ws, CreatureSnapshot cs) GenerateSnapshots(Creature creature)
        {
            return (new WorldSnapshot(creature.Locale.LocaleMap), new CreatureSnapshot(creature.Position));
        }
    }
}
