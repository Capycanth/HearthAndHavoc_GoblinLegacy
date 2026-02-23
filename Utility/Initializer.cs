using HearthAndHavoc_GoblinLegacy.Utility;
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
        private static readonly Random random = new();

        public static World CreateTestWorld(int kremlitCount)
        {
            World world = new();
            Locale locale = new("Locale1", CreateTestKremlits(kremlitCount), CreateEmptyMap());
            world.CurrentLocaleId = locale.Id;
            world.LocalesById.Add(locale.Id, locale);
            return world;
        }

        public static List<Kremlit> CreateTestKremlits(int count)
        {
            List<Kremlit> kremlits = new List<Kremlit>();
            for (int i = 0; i < count; i++)
            {
                string id = $"Kremlit{i + 1}";
                Texture2D texture = ContentLoader.GetTexture(i % 2 == 0 ? "Kremlit_Male" : "Kremlit_Female");
                kremlits.Add(new Kremlit(id, texture));
            }
            return kremlits;
        }

        public static MeterTile[,] CreateEmptyMap()
        {
            MeterTile[,] map = new MeterTile[1000, 1000];
            for (int y = 0; y < 1000; y++)
            {
                for (int x = 0; x < 1000; x++)
                {
                    map[y,x] = new MeterTile(ContentLoader.GetTexture("Tile_Grass"), new Point(x * 16, y * 16));
                    if (random.NextDouble() < 0.1)
                    {
                        map[y,x].Impassible = true;
                    }
                }
            }
            return map;
        }
    }
}
