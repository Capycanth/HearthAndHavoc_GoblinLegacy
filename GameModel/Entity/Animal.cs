using HearthAndHavoc_GoblinLegacy.AI.Action;
using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using HearthAndHavoc_GoblinLegacy.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Entity
{
    public class Animal : Creature
    {
        private const int WanderRadius = 16;

        public FaunaDef Def { get; }
        public float WeightKg { get; set; }
        public int StageIndex { get; private set; }

        public Animal(int id, FaunaDef def, Locale locale, float weightKg)
            : base(id, def.GrowthStages[GrowthStage.GetStageIndex(def.GrowthStages, weightKg)].Size, locale, null)
        {
            Def = def;
            WeightKg = weightKg;
            StageIndex = GrowthStage.GetStageIndex(Def.GrowthStages, WeightKg);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D texture = ContentLoader.GetTexture(Def.GrowthStages[StageIndex].TextureKey);
            spriteBatch.Draw(texture, GeoPosition, Def.TintColor);
        }

        protected override BaseAction ChooseAction()
        {
            int x = Position.X + SimRandom.Instance.Next(-WanderRadius, WanderRadius + 1);
            int y = Position.Y + SimRandom.Instance.Next(-WanderRadius, WanderRadius + 1);
            return new WalkTo(new Point(x, y));
        }

        public override float GetMoveSpeed(Activity activity) => activity switch
        {
            Activity.WALKING => Def.WalkSpeed,
            Activity.RUNNING => Def.RunSpeed,
            _ => throw new ArgumentOutOfRangeException(nameof(activity), activity, "Animal has no move speed for this activity.")
        };
    }
}
