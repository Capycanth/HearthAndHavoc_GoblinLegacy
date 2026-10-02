using HearthAndHavoc_GoblinLegacy.GameModel.Map;
using HearthAndHavoc_GoblinLegacy.Utility.Map;
using Microsoft.Xna.Framework;
using System;

namespace HearthAndHavoc_GoblinLegacy.AI.Scan
{
    // Base for scanners that read tile data from the chunks (cover, plants, water, open tiles). It checks tiles in
    // square rings moving outward from the center, so the first ring holding a reachable match is the nearest
    // one, measured in ring (Chebyshev) distance. Subclasses only decide whether one tile matches.
    public abstract class TileGridScanner : Scanner
    {
        public override Target FindNearest(TileMap map, Point center, int range)
        {
            for (int ring = 0; ring <= range; ring++)
            {
                for (int dy = -ring; dy <= ring; dy++)
                {
                    // The top and bottom rows of a ring are visited in full; the rows between them only at their
                    // two edge columns, which is where the ring passes through them.
                    bool isEdgeRow = dy == -ring || dy == ring;
                    int step = isEdgeRow ? 1 : Math.Max(1, 2 * ring);

                    for (int dx = -ring; dx <= ring; dx += step)
                    {
                        Point tile = new Point(center.X + dx, center.Y + dy);
                        if (!map.Chunks.TryGetValue(TileMap.ToChunkCoord(tile), out MapChunk chunk)) continue;

                        int localX = tile.X & MapChunk.LocalMask;
                        int localY = tile.Y & MapChunk.LocalMask;
                        Target match = TryMatch(map, chunk, localX, localY, tile);
                        if (match != null && MapUtil.TryGetStandTile(map, tile, center, out _)) return match;
                    }
                }
            }

            return null;
        }

        // Returns a target for this tile if it matches, otherwise null.
        protected abstract Target TryMatch(TileMap map, MapChunk chunk, int localX, int localY, Point tile);
    }
}
