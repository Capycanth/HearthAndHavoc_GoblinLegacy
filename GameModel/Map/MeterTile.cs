using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Map
{
    public class MeterTile : GameObject
    {
        public bool Impassible { get; set; }

        public MeterTile(Texture2D texture, Point position) : base(texture)
        {
            Impassible = false;
            Position = position;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Impassible) 
                spriteBatch.Draw(Texture, new Vector2(Position.X, Position.Y), Color.White);
        }

        public override void Update()
        {
            
        }
    }
}
