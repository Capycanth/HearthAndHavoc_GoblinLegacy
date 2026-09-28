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
        public List<Creature> Creatures { get; private set; }
        public TileMap LocaleMap { get; private set; }

        public Locale(string id, TileMap localeMap) 
        {
            Id = id;
            Creatures = new List<Creature>();
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
            foreach (Creature creature in Creatures)
            {
                creature.Update();
            }
        }

        private void PartialUpdate()
        {

        }

        public void Draw(SpriteBatch spriteBatch, Rectangle visibleTiles)
        {
            for (int tileY = visibleTiles.Top; tileY < visibleTiles.Bottom; tileY++)
            {
                for (int tileX = visibleTiles.Left; tileX < visibleTiles.Right; tileX++)
                {
                    if (!LocaleMap.Chunks.TryGetValue(TileMap.ToChunkCoord(new Point(tileX, tileY)), out MapChunk chunk)) continue;

                    int localX = tileX & MapChunk.LocalMask;
                    int localY = tileY & MapChunk.LocalMask;

                    string textureKey;
                    Color tint;

                    WaterDef water = chunk.Water[localY, localX];
                    CoverDef cover = chunk.Cover[localY, localX];
                    if (water != null)
                    {
                        textureKey = water.TextureKey;
                        tint = water.TintColor;
                    }
                    else if (cover != null)
                    {
                        textureKey = cover.TextureKey;
                        tint = cover.TintColor;
                    }
                    else
                    {
                        TerrainDef ground = chunk.Ground[localY, localX];
                        textureKey = ground.TextureKey;
                        tint = ground.TintColor;
                    }

                    Vector2 pixelPosition = new Vector2(tileX * 16, tileY * 16);
                    spriteBatch.Draw(ContentLoader.GetTexture(textureKey), pixelPosition, tint);
                }
            }

            foreach (Creature creature in Creatures)
            {
                creature.Draw(spriteBatch);
            }
        }
    }
}
