using HeartAndHavoc_GoblinLegacy.AI.AsyncProcessor;
using HearthAndHavoc_GoblinLegacy;
using HearthAndHavocGoblinLegacy.GameModel.Entity;
using HearthAndHavocGoblinLegacy.GameModel.Map;
using System;

using static HeartAndHavoc_GoblinLegacy.AI.AsyncProcessor.ProcessorThread;

namespace HearthAndHavocGoblinLegacy.AI.Action
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
