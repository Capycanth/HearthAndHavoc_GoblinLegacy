using HearthAndHavoc_GoblinLegacy.AI.Action;
using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Entity
{
    public class Animal : Creature
    {
        private const int WanderRadius = 16;

        private Random random = new();
        public FaunaDef Def { get; }
        public float WeightKg { get; set; }
        public int StageIndex { get; private set; }

        public Animal(int id, FaunaDef def, Locale locale, float weightKg, Texture2D texture)
            : base(id, def.GrowthStages[GrowthStage.GetStageIndex(def.GrowthStages, weightKg)].Size, locale, texture)
        {
            Def = def;
            WeightKg = weightKg;
            StageIndex = GrowthStage.GetStageIndex(Def.GrowthStages, WeightKg);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, GeoPosition, Def.TintColor);
        }

        protected override BaseAction ChooseAction()
        {
            int x = Position.X + random.Next(-WanderRadius, WanderRadius + 1);
            int y = Position.Y + random.Next(-WanderRadius, WanderRadius + 1);
            return new GoTo(new Point(x, y));
        }
    }
}
