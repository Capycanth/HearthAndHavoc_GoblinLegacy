using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HearthAndHavocGoblinLegacy.GameModel
{
    public abstract class GameObject
    {
        private Point _position;
        public Point Position
        {
            get => _position;
            set
            {
                _position = value;
                GeoPosition = new Vector2(_position.X << 4, _position.Y << 4);
            }
        }

        public Vector2 GeoPosition { get; set; }
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
