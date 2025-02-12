using System.Collections.Generic;

namespace SG.RSC
{
    public class Analytic : AnalyticSG
    {
        public static void EventLevelUp(int level)
        {
            EventPropertiesImportant(AnalyticsManager.Names.Level, new Dictionary<string, object> {
            { AnalyticsManager.Names.Level, level },
            { "FirstDate", user.firstDate },
            { "FirstVersion", user.firstVersion },
            { "Coins", user.coins },
            { "Spins", user.spins },
            { "PermanentRecord", user.permanentRecord },
            { "Invited", user.invitedFriends.Count },
            { "MaxCatLevel", user.maxCatLevel },
            { "AverageCatLevel", user.averageCatLevel },
            { "Collection", user.collection.Count },
        });
        }

        public static void EventAchievement(string achievement)
        {
            EventPropertiesImportant(AnalyticsManager.Names.Achievements, new Dictionary<string, object> {
            { AnalyticsManager.Names.Name, achievement },
            { AnalyticsManager.Names.Level, user.level },
        });
        }
    }
}