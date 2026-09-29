using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Entity
{
    public class Plant : GameObject
    {
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

            StageIndex = GrowthStage.GetStageIndex(Def.GrowthStages, WeightKg);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, GeoPosition, Def.TintColor);
        }
    }
}
