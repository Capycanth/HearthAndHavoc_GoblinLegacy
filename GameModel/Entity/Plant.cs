using HearthAndHavoc_GoblinLegacy.Defs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Entity
{
    public class Plant : GameObject
    {
        public FloraDef Def { get; }

        public Plant(FloraDef def, Point tile, Texture2D texture) : base(texture)
        {
            Def = def;
            Position = tile;
        }

        public override void Update()
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, GeoPosition, Def.TintColor);
        }
    }
}
