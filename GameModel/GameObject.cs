using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HearthAndHavocGoblinLegacy.GameModel
{
    public abstract class GameObject
    {
        public Point Position { get; set; }
        public bool Visible { get; set; }
        public Texture2D Texture { get; private set; }

        public GameObject(Texture2D texture)
        {
            Texture = texture;
        }

        public abstract void Update();
        public abstract void Draw(SpriteBatch spriteBatch);
    }
}
