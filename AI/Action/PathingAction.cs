using HearthAndHavoc_GoblinLegacy.AI.AsyncProcessor;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using System;

using static HearthAndHavoc_GoblinLegacy.AI.AsyncProcessor.ProcessorThread;

namespace HearthAndHavoc_GoblinLegacy.AI.Action
{
    // Base for actions that ask the AI thread for an A* path (Milestone 7, decision 3).
    public abstract class PathingAction : BaseAction
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
        protected abstract (WorldSnapshot ws, CreatureSnapshot cs) GenerateSnapshots(Creature creature);
    }
}
