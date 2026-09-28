using HearthAndHavoc_GoblinLegacy.Enumeration;

namespace HearthAndHavoc_GoblinLegacy.GameModel
{
    public class SimClock
    {
        // Calendar Config
        public const int MinutesPerHour = 60;
        public const int HoursPerDay = 24;
        public const int DaysPerSeason = 28;
        public const int SeasonsPerYear = 4;

        public const int MinutesPerDay = MinutesPerHour * HoursPerDay;
        public const int DaysPerYear = DaysPerSeason * SeasonsPerYear;

        // 1 tick = 1 game minute. Tick 0 is Spring, day 1, 00:00, year 1.
        public int TotalTicks { get; private set; }

        public SimClock(int startTick)
        {
            TotalTicks = startTick;
        }

        public void Advance()
        {
            TotalTicks++;
        }

        public int Minute => TotalTicks % MinutesPerHour;
        public int Hour => TotalTicks / MinutesPerHour % HoursPerDay;
        public int TotalDays => TotalTicks / MinutesPerDay;
        public int DayOfYear => TotalDays % DaysPerYear + 1;
        public int DayOfSeason => TotalDays % DaysPerSeason + 1;
        public Season Season => (Season)(TotalDays / DaysPerSeason % SeasonsPerYear);
        public int Year => TotalDays / DaysPerYear + 1;
    }
}
