using Microsoft.Xna.Framework.Graphics;
using System;

namespace HeartAndHavoc_GoblinLegacy.GameModel.Items
{
    public class StackableItem : BaseItem
    {
        public int Count { get; set; }

        public StackableItem(Texture2D texture, string flavor) : base(texture, flavor)
        {
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
