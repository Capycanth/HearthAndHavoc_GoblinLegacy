using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.GameModel;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace HearthAndHavoc_GoblinLegacy.Utility
{
    public static class Initializer
    {
        private const int AnimalSpawnAreaSize = 64;
        private const int TestWorldSeed = 1234;

        public static World CreateTestWorld()
        {
            SimRandom.Initialize(TestWorldSeed);

            World world = new();
            Locale locale = new("Locale1", CreateStartingMap(), world.Clock);
            world.CurrentLocaleId = locale.Id;
            world.LocalesById.Add(locale.Id, locale);

            HashSet<Point> usedTiles = new();
            AddTestPlants(locale, usedTiles);

            AddTestAnimals(world, locale, usedTiles, "fauna_mammal_rabbit", 30);
            AddTestAnimals(world, locale, usedTiles, "fauna_mammal_wolf", 10);
            AddTestAnimals(world, locale, usedTiles, "fauna_mammal_deer", 20);
            AddTestAnimals(world, locale, usedTiles, "fauna_mammal_boar", 8);
            return world;
        }

        public static void AddTestPlants(Locale locale, HashSet<Point> usedTiles)
        {
            FloraDef tree = DefRegistry.Get<FloraDef>("flora_tree_apple");
            FloraDef bush = DefRegistry.Get<FloraDef>("flora_bush_berry");
            FloraDef mushroom = DefRegistry.Get<FloraDef>("flora_fungus_oyster");
            FloraDef dandelion = DefRegistry.Get<FloraDef>("flora_flower_dandelion");
            Texture2D texture = ContentLoader.GetTexture("Tile_Grass");

            Point[] treeTiles = [new(30, 4), new(33, 7), new(29, 9)];
            Point[] bushTiles = [new(40, 14), new(43, 16), new(12, 24), new(18, 28)];
            Point[] mushroomTiles = [new(31, 4), new(34, 7), new(30, 9)];
            Point[] dandelionTiles = [new(22, 20), new(25, 22), new(27, 18), new(38, 26), new(45, 8), new(50, 30)];

            foreach (Point tile in treeTiles)
            {
                AddTestPlant(locale, usedTiles, tree, tile, texture);
            }

            foreach (Point tile in bushTiles)
            {
                AddTestPlant(locale, usedTiles, bush, tile, texture);
            }

            foreach (Point tile in mushroomTiles)
            {
                AddTestPlant(locale, usedTiles, mushroom, tile, texture);
            }

            foreach (Point tile in dandelionTiles)
            {
                AddTestPlant(locale, usedTiles, dandelion, tile, texture);
            }
        }

        private static void AddTestPlant(Locale locale, HashSet<Point> usedTiles, FloraDef def, Point tile, Texture2D texture)
        {
            int ageDays = SimRandom.Instance.Next((int)MathF.Ceiling(def.FullRateMaturityDays), def.LifespanDays);
            int birthTick = locale.Clock.TotalTicks - ageDays * SimClock.MinutesPerDay;

            Plant plant = new(def, locale, tile, birthTick, def.FinalWeightKg, texture);
            plant.FoliageGrams = def.MaxFoliageGrams;
            plant.FruitCount = def.FruitMaxCount;

            locale.QueueAddPlant(plant);
            usedTiles.Add(tile);
        }

        public static void AddTestAnimals(World world, Locale locale, HashSet<Point> usedTiles, string defKey, int count)
        {
            FaunaDef def = DefRegistry.Get<FaunaDef>(defKey);
            Texture2D texture = ContentLoader.GetTexture("Kremlit_Male");

            for (int i = 0; i < count; i++)
            {
                Point tile;
                do
                {
                    tile = new Point(SimRandom.Instance.Next(AnimalSpawnAreaSize), SimRandom.Instance.Next(AnimalSpawnAreaSize));
                }
                while (usedTiles.Contains(tile) || !locale.LocaleMap.IsPassable(tile));

                usedTiles.Add(tile);
                Animal animal = new(world.NextCreatureId(), def, locale, def.FinalWeightKg, texture);
                animal.Position = tile;
                locale.QueueAdd(animal);
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
