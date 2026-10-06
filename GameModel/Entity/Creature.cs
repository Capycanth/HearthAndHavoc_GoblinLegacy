using HearthAndHavoc_GoblinLegacy.AI.Chain;
using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using HearthAndHavoc_GoblinLegacy.GameModel.Nutrition;
using HearthAndHavoc_GoblinLegacy.Utility;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics.CodeAnalysis;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Entity
{
    // Anything alive that moves and acts: animals and Kremlits. Every creature has a body (BodyDef) with weight,
    // growth stages and a Metabolism, and dies into a carcass.
    public abstract class Creature : GameObject
    {
        private bool isDead = false;

        public int Id { get; }
        public byte Size { get; set; }
        public Locale Locale { get; }
        public BodyDef Body { get; }
        public float WeightKg { get; set; }
        public int StageIndex { get; private set; }
        public Metabolism Metabolism { get; }
        public int UpdateSlot { get; }
        [AllowNull]
        public ActionChain CurrentChain { get; set; }
        public CreatureActivity Activity { get; set; } = CreatureActivity.RESTING;

        // Whether the creature may run right now: not while exhausted (Milestone 7, decision 18).
        public bool CanRun => !Metabolism.IsExhausted;

        protected Creature(int id, BodyDef body, Locale locale, float weightKg)
            : base(null)
        {
            Id = id;
            Body = body;
            Locale = locale;
            WeightKg = weightKg;
            StageIndex = GrowthStage.GetStageIndex(body.GrowthStages, weightKg);
            Size = body.GrowthStages[StageIndex].Size;
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
                    Die();
                    return;
                }
            }

            CurrentChain ??= ChooseChain();

            ChainStatus status = CurrentChain.Perform(this);
            if (status != ChainStatus.RUNNING)
            {
                Console.WriteLine($"Creature {Id} finished {CurrentChain.GetType().Name}: {status}");
                CurrentChain = null;
            }

            Metabolism.UpdateStamina();
        }

        // Removes the creature from the map and leaves its carcass where it fell. Safe to call more than once in a
        // tick (for example killed and starved together): only the first call does anything. The carcass is built
        // now, while Position is still the real tile; QueueRemove only moves it later, in ProcessPending.
        public void Die()
        {
            if (isDead) return;
            isDead = true;

            Locale.QueueRemove(this);
            Locale.QueueAddCarcass(new Carcass(this));
        }

        // Adds up to kg of lean weight, never past the current stage's maturity weight. On reaching it, the
        // creature moves to the next stage only if the tile has room for the larger size; otherwise it stays at
        // the mark and tries again on later updates. Returns the kg actually grown.
        public float Grow(float kg)
        {
            float stageMaturityKg = Body.GrowthStages[StageIndex].StageMaturityWeightKg;
            float grownKg = MathF.Max(0f, MathF.Min(kg, stageMaturityKg - WeightKg));
            WeightKg += grownKg;

            bool isFinalStage = StageIndex == Body.GrowthStages.Count - 1;
            if (!isFinalStage && WeightKg >= stageMaturityKg && Locale.TryResizeCreature(this, Body.GrowthStages[StageIndex + 1].Size))
            {
                StageIndex++;
            }

            return grownKg;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D texture = ContentLoader.GetTexture(Body.GrowthStages[StageIndex].TextureKey);
            spriteBatch.Draw(texture, GeoPosition, Body.TintColor);
        }

        public float GetMoveSpeed(CreatureActivity activity) => activity switch
        {
            CreatureActivity.WALKING => Body.WalkSpeed,
            CreatureActivity.RUNNING => Body.RunSpeed,
            CreatureActivity.CROUCHING => Body.WalkSpeed,
            _ => throw new ArgumentOutOfRangeException(nameof(activity), activity, "Creature has no move speed for this activity.")
        };

        protected abstract ActionChain ChooseChain();
    }
}
