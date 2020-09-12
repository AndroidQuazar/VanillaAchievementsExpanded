using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
    public static class DebugTools
    {
        internal const string VAEDebugCategory = "Vanilla Achievements Expanded";

        [DebugAction(VAEDebugCategory, null)]
        private static void UnlockAchievement()
        {
            List<DebugMenuOption> list = new List<DebugMenuOption>();
            var lockedAchievements = AchievementPointManager.AchievementList.Where(c => !c.unlocked);
            if (!lockedAchievements.EnumerableNullOrEmpty())
            {
                foreach (AchievementCard card in lockedAchievements.OrderBy(a => a.def.defName))
			    {
				    list.Add(new DebugMenuOption(card.def.defName, DebugMenuOptionMode.Action, delegate()
				    {
                        card.UnlockCard();
				    }));
			    }
			    Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
            }
            else
            {
                Messages.Message("No Achievements To Unlock", MessageTypeDefOf.RejectInput);
            }
        }

        [DebugAction(VAEDebugCategory, null)]
        private static void UnlockAllAchievements()
        {
            foreach (AchievementCard card in AchievementPointManager.AchievementList)
            {
                card.UnlockCard();
            }
        }

        [DebugAction(VAEDebugCategory, null)]
        private static void LockAllAchievements()
        {
            foreach (AchievementCard card in AchievementPointManager.AchievementList)
            {
                card.LockCard();
            }

            Current.Game.GetComponent<AchievementPointManager>().ResetPoints();
        }

        [DebugAction(VAEDebugCategory, null)]
        private static void RegenerateAllAchievements()
        {
            Current.Game.GetComponent<AchievementPointManager>().HardReset();
        }
    }
}
