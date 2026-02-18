using HearthAndHavoc_GoblinLegacy.AI.AsyncProcessor;
using HearthAndHavoc_GoblinLegacy;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using System;

using static HearthAndHavoc_GoblinLegacy.AI.AsyncProcessor.ProcessorThread;

namespace HearthAndHavoc_GoblinLegacy.AI.Action
{
    public abstract class BaseAction
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

        public abstract bool Perform(World world, Kremlit kremlit);
        protected abstract void CalculateActionChain(WorldSnapshot ws, KremlitSnapshot ks);
        protected abstract (WorldSnapshot ws, KremlitSnapshot ks) GenerateSnapshots(World world, Kremlit kremlit);
    }
}
