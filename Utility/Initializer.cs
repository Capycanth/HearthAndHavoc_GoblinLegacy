using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HearthAndHavoc_GoblinLegacy.Utility
{
    public static class Initializer
    {
        public static World CreateTestWorld(int kremlitCount)
        {
            World world = new();
            Locale locale = new("Locale1", CreateStartingMap());
            world.CurrentLocaleId = locale.Id;
            world.LocalesById.Add(locale.Id, locale);
            AddTestKremlits(world, locale, kremlitCount);
            return world;
        }

        public static void AddTestKremlits(World world, Locale locale, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Texture2D texture = ContentLoader.GetTexture(i % 2 == 0 ? "Kremlit_Male" : "Kremlit_Female");
                locale.Creatures.Add(new Kremlit(world.NextCreatureId(), locale, texture));
            }
        }

        public static TileMap CreateStartingMap()
        {
            TileMap map = new();
            MapChunk chunk = map.GetOrCreateChunk(Point.Zero);

            TerrainDef sand = DefRegistry.Get<TerrainDef>("terrain_sand");
            TerrainDef rock = DefRegistry.Get<TerrainDef>("terrain_rock");
            WaterDef lakeWater = DefRegistry.Get<WaterDef>("water_lake");
            CoverDef clover = DefRegistry.Get<CoverDef>("cover_clover");

            // Pond: lake water over sand, 0.3 m deep at the edge to 1.5 m in the centre, with a dry sand shore
            Vector2 pondCentre = new(10, 6);
            const float pondRadius = 4f;
            for (int y = 1; y <= 11; y++)
            {
                for (int x = 5; x <= 15; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), pondCentre);
                    if (distance <= pondRadius)
                    {
                        chunk.Ground[y, x] = sand;
                        chunk.SetCover(x, y, null);
                        chunk.Water[y, x] = lakeWater;
                        chunk.WaterDepth[y, x] = (byte)(3 + (pondRadius - distance) * 3);
                    }
                    else if (distance <= pondRadius + 1)
                    {
                        chunk.Ground[y, x] = sand;
                        chunk.SetCover(x, y, null);
                    }
                }
            }

            // Rock ridge: columns 16-17, rows 0-8
            for (int y = 0; y <= 8; y++)
            {
                for (int x = 16; x <= 17; x++)
                {
                    chunk.Ground[y, x] = rock;
                    chunk.SetCover(x, y, null);
                }
            }

            // Bare dirt patch: x 1-4, y 1-3
            for (int y = 1; y <= 3; y++)
            {
                for (int x = 1; x <= 4; x++)
                {
                    chunk.SetCover(x, y, null);
                }
            }

            // Clover patch: x 20-24, y 10-13
            for (int y = 10; y <= 13; y++)
            {
                for (int x = 20; x <= 24; x++)
                {
                    chunk.SetCover(x, y, clover);
                }
            }

            return map;
        }
    }
}
