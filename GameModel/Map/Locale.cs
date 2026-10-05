using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using HearthAndHavoc_GoblinLegacy.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Map
{
    public class Locale
    {
        private const int CellShift = 5;
        private const int PlantUpdateSlots = SimClock.MinutesPerHour;

        public string Id { get; private set; }
        public IReadOnlyList<Creature> Creatures => creatures;
        public TileMap LocaleMap { get; private set; }
        public SimClock Clock { get; }

        private readonly CoverGrowth coverGrowth;
        private readonly List<Plant>[] plantSlots = new List<Plant>[PlantUpdateSlots];
        private int nextPlantSlot = 0;
        private readonly List<Creature> creatures = new();
        private readonly Dictionary<Point, List<Creature>> creatureCells = new();
        private readonly List<Creature> pendingAdds = new();
        private readonly List<Creature> pendingRemoves = new();
        private readonly List<Plant> pendingPlantAdds = new();
        private readonly List<Plant> pendingPlantRemoves = new();
        private readonly List<Creature> visibleCreatures = new();
        private readonly List<Carcass> carcasses = new();
        private readonly Dictionary<Point, List<Carcass>> carcassCells = new();
        private readonly List<Carcass> pendingCarcassAdds = new();
        private readonly List<Carcass> pendingCarcassRemoves = new();
        private readonly List<Carcass> visibleCarcasses = new();

        public Locale(string id, TileMap localeMap, SimClock clock)
        {
            Id = id;
            LocaleMap = localeMap;
            Clock = clock;

            for (int i = 0; i < PlantUpdateSlots; i++)
            {
                plantSlots[i] = new List<Plant>();
            }

            coverGrowth = new CoverGrowth(localeMap);
            foreach (MapChunk chunk in localeMap.Chunks.Values)
            {
                coverGrowth.AddBareTiles(chunk);
            }
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
            if (Clock.Hour == 0 && Clock.Minute == 0)
            {
                coverGrowth.UpdateDay(Clock.Season);
            }

            foreach (Plant plant in plantSlots[Clock.Minute])
            {
                plant.Update();
            }

            foreach (Creature creature in creatures)
            {
                creature.Update();
            }

            // Carcasses only change state with time, so once an hour is enough to clear the decomposed ones.
            if (Clock.Minute == 0)
            {
                foreach (Carcass carcass in carcasses)
                {
                    if (carcass.IsDecomposed) QueueRemoveCarcass(carcass);
                }
            }

            ProcessPending();
        }

        private void PartialUpdate()
        {

        }

        public void QueueAdd(Creature creature)
        {
            pendingAdds.Add(creature);
        }

        public void QueueRemove(Creature creature)
        {
            pendingRemoves.Add(creature);
        }

        public void QueueAddCarcass(Carcass carcass)
        {
            pendingCarcassAdds.Add(carcass);
        }

        public void QueueRemoveCarcass(Carcass carcass)
        {
            pendingCarcassRemoves.Add(carcass);
        }

        public void QueueAddPlant(Plant plant)
        {
            pendingPlantAdds.Add(plant);
        }

        public void QueueRemovePlant(Plant plant)
        {
            pendingPlantRemoves.Add(plant);
        }

        private void ProcessPending()
        {
            foreach (Plant plant in pendingPlantRemoves)
            {
                MapChunk chunk = LocaleMap.Chunks[TileMap.ToChunkCoord(plant.Position)];
                int localX = plant.Position.X & MapChunk.LocalMask;
                int localY = plant.Position.Y & MapChunk.LocalMask;

                if (chunk.Plants[localY, localX] == plant)
                {
                    chunk.Plants[localY, localX] = null;
                    plantSlots[plant.UpdateSlot].Remove(plant);
                    coverGrowth.OnTileBared(plant.Position);
                    plant.Position = GameObject.RemovedPosition;
                }
            }
            pendingPlantRemoves.Clear();

            foreach (Plant plant in pendingPlantAdds)
            {
                if (!LocaleMap.Chunks.TryGetValue(TileMap.ToChunkCoord(plant.Position), out MapChunk chunk))
                {
                    Debug.WriteLine($"Plant {plant.Def.Key} was not added: no chunk at {plant.Position}");
                    continue;
                }

                int localX = plant.Position.X & MapChunk.LocalMask;
                int localY = plant.Position.Y & MapChunk.LocalMask;

                if (chunk.Plants[localY, localX] != null)
                {
                    Debug.WriteLine($"Plant {plant.Def.Key} was not added: tile {plant.Position} already has a plant");
                    continue;
                }

                chunk.SetCover(localX, localY, null);
                chunk.Plants[localY, localX] = plant;
                coverGrowth.OnPlantPlaced(plant.Position);

                plant.UpdateSlot = nextPlantSlot;
                plantSlots[nextPlantSlot].Add(plant);
                nextPlantSlot = (nextPlantSlot + 1) % PlantUpdateSlots;
            }
            pendingPlantAdds.Clear();

            foreach (Creature creature in pendingRemoves)
            {
                if (!creatures.Remove(creature)) continue;

                LocaleMap.AddOccupancy(creature.Position, -creature.Size);
                RemoveFromCell(creatureCells, creature, creature.Position);
                creature.Position = GameObject.RemovedPosition;
            }
            pendingRemoves.Clear();

            foreach (Creature creature in pendingAdds)
            {
                if (!LocaleMap.CanFit(creature.Position, creature.Size))
                {
                    Debug.WriteLine($"Creature {creature.Id} was not added: tile {creature.Position} is full");
                    continue;
                }

                creatures.Add(creature);
                LocaleMap.AddOccupancy(creature.Position, creature.Size);
                AddToCell(creatureCells, creature, creature.Position);
            }
            pendingAdds.Clear();

            foreach (Carcass carcass in pendingCarcassRemoves)
            {
                if (!carcasses.Remove(carcass)) continue;

                RemoveFromCell(carcassCells, carcass, carcass.Position);
                carcass.Position = GameObject.RemovedPosition;
            }
            pendingCarcassRemoves.Clear();

            // Carcasses take no tile capacity and don't block movement, so any tile can hold one.
            foreach (Carcass carcass in pendingCarcassAdds)
            {
                carcasses.Add(carcass);
                AddToCell(carcassCells, carcass, carcass.Position);
            }
            pendingCarcassAdds.Clear();
        }

        public int GrazeCover(Point tile, int grams)
        {
            if (!LocaleMap.Chunks.TryGetValue(TileMap.ToChunkCoord(tile), out MapChunk chunk)) return 0;

            int localX = tile.X & MapChunk.LocalMask;
            int localY = tile.Y & MapChunk.LocalMask;
            if (chunk.Cover[localY, localX] == null) return 0;

            int biomass = chunk.CoverBiomass[localY, localX];
            int eaten = Math.Min(grams, biomass);
            if (eaten == biomass)
            {
                chunk.SetCover(localX, localY, null);
                coverGrowth.OnTileBared(tile);
            }
            else
            {
                chunk.CoverBiomass[localY, localX] = (ushort)(biomass - eaten);
                coverGrowth.OnCoverLowered(tile);
            }

            return eaten;
        }

        public bool MoveCreature(Creature creature, Point tile)
        {
            Point oldTile = creature.Position;
            if (tile == oldTile) return true;
            if (!LocaleMap.IsPassable(tile)) return false;
            if (!LocaleMap.CanFit(tile, creature.Size)) return false;

            LocaleMap.AddOccupancy(oldTile, -creature.Size);
            LocaleMap.AddOccupancy(tile, creature.Size);

            if (ToCell(oldTile) != ToCell(tile))
            {
                RemoveFromCell(creatureCells, creature, oldTile);
                AddToCell(creatureCells, creature, tile);
            }

            creature.Position = tile;
            return true;
        }

        public bool TryResizeCreature(Creature creature, byte newSize)
        {
            if (newSize > creature.Size && !LocaleMap.CanFit(creature.Position, (byte)(newSize - creature.Size))) return false;

            LocaleMap.AddOccupancy(creature.Position, newSize - creature.Size);
            creature.Size = newSize;
            return true;
        }

        public void GetCreaturesInArea(Rectangle tiles, List<Creature> results)
        {
            GetInArea(creatureCells, tiles, results);
        }

        public void GetCarcassesInArea(Rectangle tiles, List<Carcass> results)
        {
            GetInArea(carcassCells, tiles, results);
        }

        // The cell helpers are shared by every kind of object indexed in 32x32-tile cells (creatures, carcasses).
        // T is constrained to GameObject so the area query can read each object's Position.
        private static void GetInArea<T>(Dictionary<Point, List<T>> cells, Rectangle tiles, List<T> results) where T : GameObject
        {
            Point firstCell = ToCell(new Point(tiles.Left, tiles.Top));
            Point lastCell = ToCell(new Point(tiles.Right - 1, tiles.Bottom - 1));

            for (int cellY = firstCell.Y; cellY <= lastCell.Y; cellY++)
            {
                for (int cellX = firstCell.X; cellX <= lastCell.X; cellX++)
                {
                    if (!cells.TryGetValue(new Point(cellX, cellY), out List<T> cellItems)) continue;

                    foreach (T item in cellItems)
                    {
                        if (tiles.Contains(item.Position)) results.Add(item);
                    }
                }
            }
        }

        private static void AddToCell<T>(Dictionary<Point, List<T>> cells, T item, Point tile)
        {
            Point cell = ToCell(tile);
            if (!cells.TryGetValue(cell, out List<T> cellItems))
            {
                cellItems = new List<T>();
                cells.Add(cell, cellItems);
            }
            cellItems.Add(item);
        }

        private static void RemoveFromCell<T>(Dictionary<Point, List<T>> cells, T item, Point tile)
        {
            Point cell = ToCell(tile);
            List<T> cellItems = cells[cell];
            cellItems.Remove(item);
            if (cellItems.Count == 0) cells.Remove(cell);
        }

        private static Point ToCell(Point tile)
        {
            return new Point(tile.X >> CellShift, tile.Y >> CellShift);
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

                    chunk.Plants[localY, localX]?.Draw(spriteBatch);
                }
            }

            // Carcasses are drawn under the creatures.
            visibleCarcasses.Clear();
            GetCarcassesInArea(visibleTiles, visibleCarcasses);
            foreach (Carcass carcass in visibleCarcasses)
            {
                carcass.Draw(spriteBatch);
            }

            visibleCreatures.Clear();
            GetCreaturesInArea(visibleTiles, visibleCreatures);
            foreach (Creature creature in visibleCreatures)
            {
                creature.Draw(spriteBatch);
            }
        }
    }
}
