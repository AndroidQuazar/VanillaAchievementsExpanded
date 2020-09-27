using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace AchievementsExpanded
{
    public class Reward_RandomQuest : AchievementReward
    {
        public override bool TryExecuteEvent()
        {
            List<QuestScriptDef> quests = DefDatabase<QuestScriptDef>.AllDefsListForReading.ToList();
            Rand.PushState();
            quests.Shuffle();
            Rand.PopState();

            foreach (QuestScriptDef quest in quests)
            {
                try
                {
                    QuestUtility.GenerateQuestAndMakeAvailable(quest, StorytellerUtility.DefaultThreatPointsNow(Find.World));
                    return true;
                }
                catch
                {
                }
            }
            Messages.Message("FailedRewardEvent".Translate(defName), MessageTypeDefOf.RejectInput);
            return false;
        }
    }
}
