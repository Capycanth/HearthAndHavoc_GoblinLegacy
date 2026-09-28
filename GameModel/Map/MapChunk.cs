using HearthAndHavoc_GoblinLegacy.Defs;
using Microsoft.Xna.Framework;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Map
{
    public class MapChunk
    {
        public const int Shift = 9;
        public const int Size = 1 << Shift;
        public const int LocalMask = Size - 1;

        public Point ChunkCoord { get; private set; }
        public TerrainDef[,] Ground { get; private set; }
        public WaterDef[,] Water { get; private set; }
        public byte[,] WaterDepth { get; private set; }

        public MapChunk(Point chunkCoord)
        {
            ChunkCoord = chunkCoord;
            Ground = new TerrainDef[Size, Size];
            Water = new WaterDef[Size, Size];
            WaterDepth = new byte[Size, Size];

            TerrainDef grass = DefRegistry.Get<TerrainDef>("terrain_grass");
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    Ground[y, x] = grass;
                }
            }
        }
    }
}
