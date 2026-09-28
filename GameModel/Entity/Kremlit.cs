using HearthAndHavoc_GoblinLegacy.GameModel.Items;
using HearthAndHavoc_GoblinLegacy;
using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.AI.Action;
using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Entity
{
    public class Kremlit : Creature
    {
        private const byte KremlitSize = 30;

        private Random random = new();
        public List<BaseItem> Inventory { get; set; }
        public Dictionary<NeedType, float> Needs { get; set; }

        public Kremlit(int id, Locale locale, Texture2D texture) : base(id, KremlitSize, locale, texture)
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, GeoPosition, Color.White);
        }

        protected override BaseAction ChooseAction()
        {
            return new GoTo(new Point(this.random.Next(512), this.random.Next(512)));
        }
    }
}
