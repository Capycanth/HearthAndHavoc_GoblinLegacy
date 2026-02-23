using HearthAndHavoc_GoblinLegacy.GameModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Items
{
    public abstract class BaseItem : GameObject
    {
        public float Weight { get; set; }
        public string Flavor { get; private set; }
        private Color ColorModifier { get; set; }

        protected BaseItem(Texture2D texture, string flavor) : base(texture)
        {
            Flavor = flavor;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, GeoPosition, ColorModifier);
        }
    }
}
