using HearthAndHavoc_GoblinLegacy.Defs;
using Microsoft.Xna.Framework;
using System.Collections.Concurrent;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Map
{
    public class TileMap
    {
        public const byte MaxWadeDepth = 5;

        public ConcurrentDictionary<Point, MapChunk> Chunks { get; private set; } = new();

        public MapChunk GetOrCreateChunk(Point chunkCoord)
        {
            return Chunks.GetOrAdd(chunkCoord, coord => new MapChunk(coord));
        }

        public bool IsPassable(Point tile)
        {
            if (!Chunks.TryGetValue(ToChunkCoord(tile), out MapChunk chunk)) return false;

            int localX = tile.X & MapChunk.LocalMask;
            int localY = tile.Y & MapChunk.LocalMask;

            if (!chunk.Ground[localY, localX].Passable) return false;

            return chunk.Water[localY, localX] == null || chunk.WaterDepth[localY, localX] <= MaxWadeDepth;
        }

        public int GetMoveCost(Point tile)
        {
            MapChunk chunk = Chunks[ToChunkCoord(tile)];

            int localX = tile.X & MapChunk.LocalMask;
            int localY = tile.Y & MapChunk.LocalMask;

            WaterDef water = chunk.Water[localY, localX];
            if (water != null) return water.MoveCost;
            return chunk.Ground[localY, localX].MoveCost;
        }

        public static Point ToChunkCoord(Point tile)
        {
            return new Point(tile.X >> MapChunk.Shift, tile.Y >> MapChunk.Shift);
        }
    }
}
