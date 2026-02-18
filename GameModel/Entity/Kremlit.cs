using HeartAndHavoc_GoblinLegacy.GameModel.Items;
using HearthAndHavoc_GoblinLegacy;
using HearthAndHavocGoblinLegacy.AI.Action;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HearthAndHavocGoblinLegacy.GameModel.Entity
{
    public class Kremlit : GameObject
    {
        public string Id { get; private set; }
        [AllowNull]
        public BaseAction CurrentAction { get; set; }
        private Random random = new();
        public List<BaseItem> Inventory { get; set; }

        public Kremlit(string id, Texture2D texture) : base(texture)
        {
            Id = id;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, GeoPosition, Color.White);
        }

        public override void Update()
        {
            CurrentAction ??= new GoTo(new Point(this.random.Next(1000), this.random.Next(1000)));

            if (CurrentAction.Perform(GoblinGame.world, this))
            {
                Console.WriteLine("Kremlit " + Id + " finished GoTo");
                CurrentAction = null;
            }
        }
    }
}
