using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using HearthAndHavoc_GoblinLegacy.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics.CodeAnalysis;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Entity
{
    // What a creature leaves when it dies (Milestone 7, decision 25). It holds the meat from its CarcassYield,
    // rots once that meat spoils, and decomposes and leaves the map some days later. It stays until then even
    // when the meat is gone, so its other materials (hide and so on) can be harvested later.
    public class Carcass : GameObject
    {
        private const string MeatTag = "meat";
        private const int FallbackRotDays = 2;
        private const int DecomposeDays = 5;
        private const float GramsPerKg = 1000f;
        private const float FreshDarkening = 0.5f;
        private static readonly Color RottenTint = new Color(110, 120, 60);

        private readonly string textureKey;
        private readonly int rotTick;
        private readonly int decomposeTick;

        public BodyDef SourceDef { get; }
        public Locale Locale { get; }
        // The dead creature's weight over its body's final weight, kept for scaling harvest yields later.
        public float WeightRatio { get; }
        [AllowNull]
        public ItemDef MeatItem { get; }
        public float MeatKg { get; private set; }
        public int DeathTick { get; }

        public bool IsRotten => Locale.Clock.TotalTicks >= rotTick;
        public bool IsDecomposed => Locale.Clock.TotalTicks >= decomposeTick;

        public Carcass(Creature creature) : base(null)
        {
            SourceDef = creature.Body;
            Locale = creature.Locale;
            Position = creature.Position;
            WeightRatio = creature.WeightKg / creature.Body.FinalWeightKg;
            DeathTick = Locale.Clock.TotalTicks;
            textureKey = creature.Body.GrowthStages[creature.StageIndex].TextureKey;

            CarcassYield meat = FindMeatYield(creature.Body);
            if (meat != null)
            {
                MeatItem = meat.Item;
                MeatKg = meat.Amount * meat.Item.WeightKg * WeightRatio;
            }

            int rotDays = MeatItem != null ? MeatItem.SpoilDays : FallbackRotDays;
            rotTick = DeathTick + rotDays * SimClock.MinutesPerDay;
            decomposeTick = rotTick + DecomposeDays * SimClock.MinutesPerDay;
        }

        // Takes up to grams of meat and returns the grams taken. Rotten meat is not eaten.
        public float Bite(float grams)
        {
            if (IsRotten) return 0f;

            float taken = MathF.Min(grams, MeatKg * GramsPerKg);
            MeatKg -= taken / GramsPerKg;
            return taken;
        }

        // Nothing to do each tick: rotting is read from the clock, and Locale clears decomposed carcasses hourly.
        public override void Update()
        {
        }

        // Drawn as the dead creature turned belly up: its stage texture rotated half a turn around its centre, then
        // darkened while fresh or tinted green-brown once rotten. Drawing at GeoPosition + origin keeps the
        // rotated sprite on its own tile.
        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D texture = ContentLoader.GetTexture(textureKey);
            Color tint = IsRotten ? RottenTint : Color.Lerp(SourceDef.TintColor, Color.Black, FreshDarkening);
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            spriteBatch.Draw(texture, GeoPosition + origin, null, tint, MathHelper.Pi, origin, 1f, SpriteEffects.None, 0f);
        }

        private static CarcassYield FindMeatYield(BodyDef def)
        {
            if (def.CarcassYield == null) return null;

            foreach (CarcassYield yield in def.CarcassYield)
            {
                if (yield.Item.Tags.Contains(MeatTag)) return yield;
            }

            return null;
        }
    }
}
