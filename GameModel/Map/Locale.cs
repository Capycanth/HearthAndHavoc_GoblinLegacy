using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using HearthAndHavoc_GoblinLegacy.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Map
{
    public class Locale
    {
        public string Id { get; private set; }
        public List<Kremlit> Kremlits { get; private set; }
        public TileMap LocaleMap { get; private set; }

        public Locale(string id, List<Kremlit> kremlits, TileMap localeMap) 
        {
            Id = id;
            Kremlits = kremlits;
            LocaleMap = localeMap;
        }

        public void Update(string id)
        {
            if (id == Id) FullUpdate();
            else PartialUpdate();
            
        }

        private void FullUpdate()
        {
            //foreach (Kremlit kremlit in Kremlits)
            //{
            //    LocaleMap[kremlit.Position.Y][kremlit.Position.X].Impassible = true;
            //}
            foreach (Kremlit kremlit in Kremlits)
            {
                kremlit.Update();
            }
        }

        private void PartialUpdate()
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (MapChunk chunk in LocaleMap.Chunks.Values)
            {
                int originX = chunk.ChunkCoord.X * MapChunk.Size;
                int originY = chunk.ChunkCoord.Y * MapChunk.Size;

                for (int y = 0; y < MapChunk.Size; y++)
                {
                    for (int x = 0; x < MapChunk.Size; x++)
                    {
                        string textureKey;
                        Color tint;

                        WaterDef water = chunk.Water[y, x];
                        if (water != null)
                        {
                            textureKey = water.TextureKey;
                            tint = water.TintColor;
                        }
                        else
                        {
                            TerrainDef ground = chunk.Ground[y, x];
                            textureKey = ground.TextureKey;
                            tint = ground.TintColor;
                        }

                        Vector2 pixelPosition = new Vector2((originX + x) * 16, (originY + y) * 16);
                        spriteBatch.Draw(ContentLoader.GetTexture(textureKey), pixelPosition, tint);
                    }
                }
            }

            foreach (Kremlit kremlit in Kremlits)
            {
                kremlit.Draw(spriteBatch);
            }
        }
    }
}
