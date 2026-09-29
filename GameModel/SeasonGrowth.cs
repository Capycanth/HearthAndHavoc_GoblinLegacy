using HearthAndHavoc_GoblinLegacy.Enumeration;

namespace HearthAndHavoc_GoblinLegacy.GameModel
{
    public static class SeasonGrowth
    {
        public static float Multiplier(Season season) => season switch
        {
            Season.SPRING => 1f,
            Season.SUMMER => 1f,
            Season.AUTUMN => 0.5f,
            Season.WINTER => 0f,
            _ => 0f,
        };
    }
}
