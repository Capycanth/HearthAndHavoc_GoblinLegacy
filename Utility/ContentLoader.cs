using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace HearthAndHavoc_GoblinLegacy.Utility
{
    public static class ContentLoader
    {
        private static Dictionary<string, Texture2D> TextureCache { get; set; }

        public static void Initialize(ContentManager contentManager)
        {
            TextureCache = new Dictionary<string, Texture2D>(3)
            {
                { "Kremlit_Male", contentManager.Load<Texture2D>("texture/Goblin_Male") },
                { "Kremlit_Female", contentManager.Load<Texture2D>("texture/Goblin_Female") },
                { "Tile_Grass", contentManager.Load<Texture2D>("texture/Grass_Tile") }
            };
        }

        public static Texture2D GetTexture(string name)
        {
            if (TextureCache.TryGetValue(name, out var texture))
            {
                return texture;
            }
            else
            {
                throw new KeyNotFoundException($"Texture '{name}' not found in cache.");
            }
        }
    }
}
