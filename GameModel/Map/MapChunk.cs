using HearthAndHavoc_GoblinLegacy.Defs;
using HearthAndHavoc_GoblinLegacy.GameModel.Entity;
using Microsoft.Xna.Framework;

namespace HearthAndHavoc_GoblinLegacy.GameModel.Map
{
    public class MapChunk
    {
        public const int Shift = 9;
        public const int Size = 1 << Shift;
        public const int LocalMask = Size - 1;
        public const byte DefaultFertility = 50;

        public Point ChunkCoord { get; private set; }
        public TerrainDef[,] Ground { get; private set; }
        public WaterDef[,] Water { get; private set; }
        public byte[,] WaterDepth { get; private set; }
        public CoverDef[,] Cover { get; private set; }
        public ushort[,] CoverBiomass { get; private set; }
        public byte[,] Fertility { get; private set; }
        public byte[,] Occupancy { get; private set; }
        public Plant[,] Plants { get; private set; }

        public MapChunk(Point chunkCoord)
        {
            ChunkCoord = chunkCoord;
            Ground = new TerrainDef[Size, Size];
            Water = new WaterDef[Size, Size];
            WaterDepth = new byte[Size, Size];
            Cover = new CoverDef[Size, Size];
            CoverBiomass = new ushort[Size, Size];
            Fertility = new byte[Size, Size];
            Occupancy = new byte[Size, Size];
            Plants = new Plant[Size, Size];

            TerrainDef dirt = DefRegistry.Get<TerrainDef>("terrain_dirt");
            CoverDef grass = DefRegistry.Get<CoverDef>("cover_grass");
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    Ground[y, x] = dirt;
                    SetCover(x, y, grass);
                    Fertility[y, x] = DefaultFertility;
                }
            }
        }

        public void SetCover(int localX, int localY, CoverDef cover)
        {
            Cover[localY, localX] = cover;
            CoverBiomass[localY, localX] = cover == null ? (ushort)0 : cover.MaxBiomass;
        }
    }
}
