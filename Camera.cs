using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace HearthAndHavoc_GoblinLegacy
{
    public class Camera
    {
        private const int TileSize = 16;
        public const int MinZoom = 1;
        public const int MaxZoom = 4;

        public Vector2 Position { get; private set; } = Vector2.Zero;
        public int Zoom { get; private set; } = MaxZoom;

        public void Update(MouseState current, MouseState previous)
        {
            // Drag: move against the mouse so the map stays under the cursor
            if (current.LeftButton == ButtonState.Pressed && previous.LeftButton == ButtonState.Pressed)
            {
                Vector2 mouseMovement = (current.Position - previous.Position).ToVector2();
                Position -= mouseMovement / Zoom;
            }

            // Zoom: one step per wheel change, keeping the world point under the cursor in place
            int wheelChange = current.ScrollWheelValue - previous.ScrollWheelValue;
            int newZoom = Zoom;
            if (wheelChange > 0) newZoom = Math.Min(Zoom + 1, MaxZoom);
            else if (wheelChange < 0) newZoom = Math.Max(Zoom - 1, MinZoom);

            if (newZoom != Zoom)
            {
                Vector2 mouseOnScreen = current.Position.ToVector2();
                Vector2 worldUnderMouse = Position + mouseOnScreen / Zoom;
                Zoom = newZoom;
                Position = worldUnderMouse - mouseOnScreen / Zoom;
            }
        }

        public Matrix GetTransform()
        {
            // Rounded to whole screen pixels so tiles don't shimmer or show gaps while panning
            float offsetX = MathF.Round(Position.X * Zoom);
            float offsetY = MathF.Round(Position.Y * Zoom);
            return Matrix.CreateScale(Zoom) * Matrix.CreateTranslation(-offsetX, -offsetY, 0f);
        }

        public Rectangle GetVisibleTiles(Viewport viewport)
        {
            int left = (int)MathF.Floor(Position.X / TileSize);
            int top = (int)MathF.Floor(Position.Y / TileSize);
            int right = (int)MathF.Floor((Position.X + viewport.Width / (float)Zoom) / TileSize);
            int bottom = (int)MathF.Floor((Position.Y + viewport.Height / (float)Zoom) / TileSize);
            return new Rectangle(left, top, right - left + 1, bottom - top + 1);
        }
    }
}
