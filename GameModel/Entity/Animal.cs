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
        public BiotaDef Def { get; }

        public Animal(int id, BiotaDef def, Locale locale, Texture2D texture) : base(id, def.Size, locale, texture)
        {
            Def = def;
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
