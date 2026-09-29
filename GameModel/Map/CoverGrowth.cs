using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.Enumeration;
using HearthAndHavoc_GoblinLegacy.Utility;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Map
{
    public class CoverGrowth
    {
        private const float AverageFertility = 50f;
        private const float MaxFertility = 100f;
        private const float NewCoverBiomassShare = 0.1f;

        private readonly TileMap map;
        private readonly HashSet<Point> regrowing = new();
        private readonly HashSet<Point> spreadable = new();
        private readonly List<Point> leaving = new();
        private readonly List<(Point Tile, CoverDef Cover)> newCover = new();
        private readonly List<CoverDef> neighborCovers = new();

        public CoverGrowth(TileMap map)
        {
            this.map = map;
        }

        public void AddBareTiles(MapChunk chunk)
        {
            Point origin = new(chunk.ChunkCoord.X << MapChunk.Shift, chunk.ChunkCoord.Y << MapChunk.Shift);
            for (int localY = 0; localY < MapChunk.Size; localY++)
            {
                for (int localX = 0; localX < MapChunk.Size; localX++)
                {
                    if (IsBare(chunk, localX, localY)) spreadable.Add(new Point(origin.X + localX, origin.Y + localY));
                }
            }
        }

        public void OnCoverLowered(Point tile)
        {
            regrowing.Add(tile);
        }

        public void OnTileBared(Point tile)
        {
            regrowing.Remove(tile);
            spreadable.Add(tile);
        }

        public void OnPlantPlaced(Point tile)
        {
            regrowing.Remove(tile);
            spreadable.Remove(tile);
        }

        public void UpdateDay(Season season)
        {
            Regrow(season);
            Spread(season);
        }

        private void Regrow(Season season)
        {
            foreach (Point tile in regrowing)
            {
                if (!TryGetTile(tile, out MapChunk chunk, out int localX, out int localY))
                {
                    leaving.Add(tile);
                    continue;
                }

                CoverDef cover = chunk.Cover[localY, localX];
                if (cover == null)
                {
                    leaving.Add(tile);
                    continue;
                }

                float grams = cover.RegrowGramsPerDay * chunk.Fertility[localY, localX] / AverageFertility * cover.SeasonGrowth[season];
                int biomass = Math.Min(chunk.CoverBiomass[localY, localX] + (int)MathF.Round(grams), cover.MaxBiomass);
                chunk.CoverBiomass[localY, localX] = (ushort)biomass;

                if (biomass == cover.MaxBiomass) leaving.Add(tile);
            }

            foreach (Point tile in leaving) regrowing.Remove(tile);
            leaving.Clear();
        }

        private void Spread(Season season)
        {
            foreach (Point tile in spreadable)
            {
                if (!TryGetTile(tile, out MapChunk chunk, out int localX, out int localY) || !IsBare(chunk, localX, localY))
                {
                    leaving.Add(tile);
                    continue;
                }

                TerrainDef ground = chunk.Ground[localY, localX];
                neighborCovers.Clear();
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        if (!TryGetTile(new Point(tile.X + dx, tile.Y + dy), out MapChunk neighborChunk, out int neighborX, out int neighborY)) continue;

                        CoverDef neighborCover = neighborChunk.Cover[neighborY, neighborX];
                        if (neighborCover != null && neighborCover.Terrains.Contains(ground)) neighborCovers.Add(neighborCover);
                    }
                }

                if (neighborCovers.Count == 0)
                {
                    leaving.Add(tile);
                    continue;
                }

                CoverDef cover = neighborCovers[SimRandom.Instance.Next(neighborCovers.Count)];
                float chance = cover.SpreadChance * MathF.Min(1f, chunk.Fertility[localY, localX] / MaxFertility * cover.SeasonGrowth[season]);
                if (SimRandom.Instance.NextSingle() >= chance) continue;

                newCover.Add((tile, cover));
            }

            foreach (Point tile in leaving) spreadable.Remove(tile);
            leaving.Clear();

            foreach ((Point tile, CoverDef cover) in newCover)
            {
                TryGetTile(tile, out MapChunk chunk, out int localX, out int localY);
                chunk.SetCover(localX, localY, cover);
                chunk.CoverBiomass[localY, localX] = (ushort)(cover.MaxBiomass * NewCoverBiomassShare);

                spreadable.Remove(tile);
                regrowing.Add(tile);

                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        Point neighbor = new(tile.X + dx, tile.Y + dy);
                        if (TryGetTile(neighbor, out MapChunk neighborChunk, out int neighborX, out int neighborY) && IsBare(neighborChunk, neighborX, neighborY))
                        {
                            spreadable.Add(neighbor);
                        }
                    }
                }
            }
            newCover.Clear();
        }

        private static bool IsBare(MapChunk chunk, int localX, int localY)
        {
            return chunk.Cover[localY, localX] == null && chunk.Plants[localY, localX] == null && chunk.Water[localY, localX] == null;
        }

        private bool TryGetTile(Point tile, out MapChunk chunk, out int localX, out int localY)
        {
            localX = tile.X & MapChunk.LocalMask;
            localY = tile.Y & MapChunk.LocalMask;
            return map.Chunks.TryGetValue(TileMap.ToChunkCoord(tile), out chunk);
        }
    }
}
