using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
    public static class AchievementGenerator
    {
        public static Dictionary<TrackerBase, HashSet<AchievementCard>> GenerateAchievementLinks(HashSet<AchievementCard> cards)
        {
            var achievementList = new Dictionary<TrackerBase, HashSet<AchievementCard>>();

            foreach (AchievementCard card in cards)
            {
                var tracker = AchievementPointManager.TrackersGenerated.Single(t => t.Key == card.def.tracker.Key);
                if (!achievementList.TryGetValue(tracker, out var hash))
                {
                    achievementList.Add(tracker, new HashSet<AchievementCard>() { card });
                    continue;
                }
                hash.Add(card);
            }
            return achievementList;
        }

        public static bool VerifyAchievementList(ref HashSet<AchievementCard> achievementCards, bool debugOutput = false)
        {
            bool newlyAdded = false;
            int count = 0;
            int defCount = 0;
            foreach (AchievementDef def in DefDatabase<AchievementDef>.AllDefs)
            {
                var card = achievementCards.FirstOrDefault(a => a.def.defName == def.defName);
                if (card is null)
                {
                    card = new AchievementCard(def);
                    achievementCards.Add(card);
                    newlyAdded = true;
                    count++;
                }
                defCount++;
            }
            if(debugOutput)
                Log.Message($"[{AchievementPointManager.AchievementTag}] {count}/{defCount} achievements generated.");
            return newlyAdded;
        }

        public static IEnumerable<AchievementCard> ExtractAchievements(Dictionary<TrackerBase, HashSet<AchievementCard>> achievements)
        { 
            foreach (var hash in achievements.Values)
            {
                foreach (var card in hash)
                {
                    yield return card;
                }
            }
        }
    }
}
