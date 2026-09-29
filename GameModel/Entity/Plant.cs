using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Entity
{
    public class Plant : GameObject
    {
        private const float AverageFertility = 50f;

        public FloraDef Def { get; }
        public Locale Locale { get; }
        public int BirthTick { get; }
        public float WeightKg { get; set; }
        public int StageIndex { get; private set; }
        public float FoliageGrams { get; set; }
        public float FruitCount { get; set; }
        public int UpdateSlot { get; set; }

        public int AgeDays => (Locale.Clock.TotalTicks - BirthTick) / SimClock.MinutesPerDay;

        public Plant(FloraDef def, Locale locale, Point tile, int birthTick, float weightKg, Texture2D texture) : base(texture)
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

            float multiplier = Locale.LocaleMap.GetFertility(Position) / AverageFertility * SeasonGrowth.Multiplier(Locale.Clock.Season);

            float growthKgPerHour = Def.GrowthStages[StageIndex].GrowthKgPerDay / SimClock.HoursPerDay;
            WeightKg = MathF.Min(WeightKg + growthKgPerHour * multiplier, Def.FinalWeightKg);
            StageIndex = GrowthStage.GetStageIndex(Def.GrowthStages, WeightKg);

            if (Def.MaxFoliageGrams > 0)
            {
                float maxFoliageGrams = Def.MaxFoliageGrams * WeightKg / Def.FinalWeightKg;
                float foliageGramsPerHour = Def.FoliageRegrowGramsPerDay / SimClock.HoursPerDay;
                FoliageGrams = MathF.Min(FoliageGrams + foliageGramsPerHour * multiplier, maxFoliageGrams);
            }

            bool isMature = StageIndex == Def.GrowthStages.Count - 1;
            if (isMature && Def.FruitItem != null)
            {
                float fruitPerHour = (float)Def.FruitMaxCount / Def.FruitRegrowDays / SimClock.HoursPerDay;
                FruitCount = MathF.Min(FruitCount + fruitPerHour * multiplier, Def.FruitMaxCount);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, GeoPosition, Def.TintColor);
        }
    }
}
