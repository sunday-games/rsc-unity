namespace SG
{
    public class AnalyticsManager : Core
    {
        public static class Names
        {
            public static string Name = "Name";
            public static string Progress = "Progress";
            public static string Level = "Level";
            public static string Achievements = "Achievements";
            public static string Game = "Game";

            public static string Purchase = "Purchase";
            public static string Revenue = "Revenue";

            public static string FirstDate = "FirstDate";
            public static string FirstVersion = "FirstVersion";

            public static string Gold = "Gold";
            public static string Rhymes = "Rhymes";
        }

        public virtual void Init() { }
    }
}