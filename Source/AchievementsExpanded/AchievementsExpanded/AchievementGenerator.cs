using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
    public static class AchievementGenerator
    {
        public static IEnumerable<AchievementCard> GenerateAchievementsInit()
        {
            foreach (AchievementDef def in DefDatabase<AchievementDef>.AllDefs)
            {
                yield return new AchievementCard(def);
            }
        }

        public static bool VerifyAchievementList(ref List<AchievementCard> achievementCards)
        {
            bool newlyAdded = false;
            foreach (AchievementDef def in DefDatabase<AchievementDef>.AllDefs)
            {
                var card = achievementCards.FirstOrDefault(a => a.def.defName == def.defName);
                if (card is null)
                {
                    card = new AchievementCard(def);
                    achievementCards.Add(card);
                    newlyAdded = true;
                }
                else
                {
                    bool unlocked = card.unlocked;
                    achievementCards.Remove(card);
                    card = new AchievementCard(def, unlocked);
                    achievementCards.Add(card);
                }
            }
            return newlyAdded;
        }
    }
}
