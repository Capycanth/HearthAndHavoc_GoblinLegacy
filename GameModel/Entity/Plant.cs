using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using HearthAndHavoc_GoblinLegacy.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Entity
{
    public class Plant : GameObject
    {
        private const float AverageFertility = 50f;
        private const float MaxFertility = 100f;

        public FloraDef Def { get; }
        public Locale Locale { get; }
        public int BirthTick { get; }
        public float WeightKg { get; set; }
        public int StageIndex { get; private set; }
        public float FoliageGrams { get; set; }
        public float FruitCount { get; set; }
        public int UpdateSlot { get; set; }

        public bool BlocksMovement => Def.GrowthStages[StageIndex].BlocksMovement;
        public bool IsMature => StageIndex == Def.GrowthStages.Count - 1;

        public int AgeDays => (Locale.Clock.TotalTicks - BirthTick) / SimClock.MinutesPerDay;

        public Plant(FloraDef def, Locale locale, Point tile, int birthTick, float weightKg) : base(null)
        {
            Def = def;
            Locale = locale;
            Position = tile;
            BirthTick = birthTick;
            WeightKg = weightKg;
            StageIndex = GrowthStage.GetStageIndex(Def.GrowthStages, WeightKg);
        }

        public override void Update()
        {
            if (AgeDays >= Def.LifespanDays)
            {
                Locale.QueueRemovePlant(this);
                return;
            }

            if (Def.Hosts != null && Locale.Clock.Hour == 0 && !HasHostNearby(Position))
            {
                Locale.QueueRemovePlant(this);
                return;
            }

            Season season = Locale.Clock.Season;
            float fertility = Locale.LocaleMap.GetFertility(Position) / AverageFertility;
            float growthMultiplier = fertility * Def.SeasonGrowth[season];

            float growthKgPerHour = Def.GrowthStages[StageIndex].GrowthKgPerDay / SimClock.HoursPerDay;
            WeightKg = MathF.Min(WeightKg + growthKgPerHour * growthMultiplier, Def.FinalWeightKg);
            StageIndex = GrowthStage.GetStageIndex(Def.GrowthStages, WeightKg);

            if (Def.MaxFoliageGrams > 0)
            {
                float maxFoliageGrams = Def.MaxFoliageGrams * WeightKg / Def.FinalWeightKg;
                float foliageGramsPerHour = Def.FoliageRegrowGramsPerDay / SimClock.HoursPerDay;
                FoliageGrams = MathF.Min(FoliageGrams + foliageGramsPerHour * growthMultiplier, maxFoliageGrams);
            }

            if (IsMature && Def.FruitItem != null)
            {
                float fruitingMultiplier = fertility * Def.SeasonFruiting[season];
                float fruitPerHour = (float)Def.FruitMaxCount / Def.FruitRegrowDays / SimClock.HoursPerDay;
                FruitCount = MathF.Min(FruitCount + fruitPerHour * fruitingMultiplier, Def.FruitMaxCount);
            }

            if (IsMature && Locale.Clock.Hour == 0)
            {
                TrySpread(season);
            }
        }

        private void TrySpread(Season season)
        {
            if (SimRandom.Instance.NextSingle() >= Def.SpreadChance) return;

            int radius = Def.SpreadRadius;
            Point target = new(
                Position.X + SimRandom.Instance.Next(-radius, radius + 1),
                Position.Y + SimRandom.Instance.Next(-radius, radius + 1));

            if (!Locale.LocaleMap.Chunks.TryGetValue(TileMap.ToChunkCoord(target), out MapChunk chunk)) return;

            int localX = target.X & MapChunk.LocalMask;
            int localY = target.Y & MapChunk.LocalMask;

            if (chunk.Water[localY, localX] != null) return;
            if (!chunk.Ground[localY, localX].Passable) return;
            if (chunk.Plants[localY, localX] != null) return;
            if (Def.Hosts != null && !HasHostNearby(target)) return;

            float sproutChance = MathF.Min(1f, chunk.Fertility[localY, localX] / MaxFertility * Def.SeasonGrowth[season]);
            if (SimRandom.Instance.NextSingle() >= sproutChance) return;

            Locale.QueueAddPlant(new Plant(Def, Locale, target, Locale.Clock.TotalTicks, 0f));
        }

        private bool HasHostNearby(Point tile)
        {
            int radius = Def.HostRadius;
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    if (dx == 0 && dy == 0) continue;

                    Plant neighbor = Locale.LocaleMap.GetPlant(new Point(tile.X + dx, tile.Y + dy));
                    if (neighbor != null && neighbor.IsMature && Def.Hosts.Contains(neighbor.Def)) return true;
                }
            }

            return false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D texture = ContentLoader.GetTexture(Def.GrowthStages[StageIndex].TextureKey);
            spriteBatch.Draw(texture, GeoPosition, Def.TintColor);
        }
    }
}
