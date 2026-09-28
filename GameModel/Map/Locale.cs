using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using HearthAndHavoc_GoblinLegacy.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Map
{
    public class Locale
    {
        private const int CellShift = 5;

        public string Id { get; private set; }
        public IReadOnlyList<Creature> Creatures => creatures;
        public TileMap LocaleMap { get; private set; }

        private readonly List<Creature> creatures = new();
        private readonly Dictionary<Point, List<Creature>> creatureCells = new();
        private readonly List<Creature> pendingAdds = new();
        private readonly List<Creature> pendingRemoves = new();
        private readonly List<Creature> visibleCreatures = new();

        public Locale(string id, TileMap localeMap) 
        {
            Id = id;
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
            foreach (Creature creature in creatures)
            {
                creature.Update();
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

        private void ProcessPending()
        {
            foreach (Creature creature in pendingRemoves)
            {
                if (!creatures.Remove(creature)) continue;

                LocaleMap.AddOccupancy(creature.Position, -creature.Size);
                RemoveFromCell(creature, creature.Position);
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
                AddToCell(creature, creature.Position);
            }
            pendingAdds.Clear();
        }

        public bool MoveCreature(Creature creature, Point tile)
        {
            Point oldTile = creature.Position;
            if (tile == oldTile) return true;
            if (!LocaleMap.CanFit(tile, creature.Size)) return false;

            LocaleMap.AddOccupancy(oldTile, -creature.Size);
            LocaleMap.AddOccupancy(tile, creature.Size);

            if (ToCell(oldTile) != ToCell(tile))
            {
                RemoveFromCell(creature, oldTile);
                AddToCell(creature, tile);
            }

            creature.Position = tile;
            return true;
        }

        public void GetCreaturesInArea(Rectangle tiles, List<Creature> results)
        {
            Point firstCell = ToCell(new Point(tiles.Left, tiles.Top));
            Point lastCell = ToCell(new Point(tiles.Right - 1, tiles.Bottom - 1));

            for (int cellY = firstCell.Y; cellY <= lastCell.Y; cellY++)
            {
                for (int cellX = firstCell.X; cellX <= lastCell.X; cellX++)
                {
                    if (!creatureCells.TryGetValue(new Point(cellX, cellY), out List<Creature> cellCreatures)) continue;

                    foreach (Creature creature in cellCreatures)
                    {
                        if (tiles.Contains(creature.Position)) results.Add(creature);
                    }
                }
            }
        }

        private void AddToCell(Creature creature, Point tile)
        {
            Point cell = ToCell(tile);
            if (!creatureCells.TryGetValue(cell, out List<Creature> cellCreatures))
            {
                cellCreatures = new List<Creature>();
                creatureCells.Add(cell, cellCreatures);
            }
            cellCreatures.Add(creature);
        }

        private void RemoveFromCell(Creature creature, Point tile)
        {
            Point cell = ToCell(tile);
            List<Creature> cellCreatures = creatureCells[cell];
            cellCreatures.Remove(creature);
            if (cellCreatures.Count == 0) creatureCells.Remove(cell);
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
                }
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
