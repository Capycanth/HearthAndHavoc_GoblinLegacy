using HearthAndHavoc_GoblinLegacy.AI.Chain;
using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using HearthAndHavoc_GoblinLegacy.GameModel.Nutrition;
using HearthAndHavoc_GoblinLegacy.Utility;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Entity
{
    public class Animal : Creature
    {
        public FaunaDef Def { get; }
        public float WeightKg { get; set; }
        public int StageIndex { get; private set; }
        public Metabolism Metabolism { get; }
        public int UpdateSlot { get; }

        public Animal(int id, FaunaDef def, Locale locale, float weightKg)
            : base(id, def.GrowthStages[GrowthStage.GetStageIndex(def.GrowthStages, weightKg)].Size, locale, null)
        {
            Def = def;
            WeightKg = weightKg;
            StageIndex = GrowthStage.GetStageIndex(Def.GrowthStages, WeightKg);
            Metabolism = new Metabolism(this);
            UpdateSlot = Id % SimClock.MinutesPerHour;
        }

        public override void Update()
        {
            if (Locale.Clock.Minute == UpdateSlot)
            {
                Metabolism.UpdateHour();
                if (Metabolism.Health <= 0f)
                {
                    Locale.QueueRemove(this);
                    return;
                }
            }

            base.Update();
        }

        // Adds up to kg of lean weight, never past the current stage's maturity weight. On reaching it, the
        // animal moves to the next stage only if the tile has room for the larger size; otherwise it stays at
        // the mark and tries again on later updates. Returns the kg actually grown.
        public float Grow(float kg)
        {
            float stageMaturityKg = Def.GrowthStages[StageIndex].StageMaturityWeightKg;
            float grownKg = MathF.Max(0f, MathF.Min(kg, stageMaturityKg - WeightKg));
            WeightKg += grownKg;

            bool isFinalStage = StageIndex == Def.GrowthStages.Count - 1;
            if (!isFinalStage && WeightKg >= stageMaturityKg && Locale.TryResizeCreature(this, Def.GrowthStages[StageIndex + 1].Size))
            {
                StageIndex++;
            }

            return grownKg;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D texture = ContentLoader.GetTexture(Def.GrowthStages[StageIndex].TextureKey);
            spriteBatch.Draw(texture, GeoPosition, Def.TintColor);
        }

        protected override ActionChain ChooseChain()
        {
            return new IdleChain();
        }

        public override float GetMoveSpeed(CreatureActivity activity) => activity switch
        {
            CreatureActivity.WALKING => Def.WalkSpeed,
            CreatureActivity.RUNNING => Def.RunSpeed,
            CreatureActivity.CROUCHING => Def.WalkSpeed,
            _ => throw new ArgumentOutOfRangeException(nameof(activity), activity, "Animal has no move speed for this activity.")
        };
    }
}
