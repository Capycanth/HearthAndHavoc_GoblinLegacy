using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HearthAndHavoc_GoblinLegacy.GameModel
{
    public abstract class GameObject
    {
        // Where an object is moved once Locale has removed it from the map, so every range check treats it as gone
        // (Milestone 7, decision 16). A quarter of int.MinValue is far from any real tile yet leaves room for
        // subtracting positions without overflowing. Its GeoPosition wraps to (0, 0), which is harmless because
        // removed objects are never drawn.
        public static readonly Point RemovedPosition = new Point(int.MinValue / 4, int.MinValue / 4);

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
