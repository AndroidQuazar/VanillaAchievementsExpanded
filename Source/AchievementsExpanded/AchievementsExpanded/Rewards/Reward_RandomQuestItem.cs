using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using RimWorld.Planet;
using Verse;
using HarmonyLib;

namespace AchievementsExpanded
{
    public class Reward_RandomQuestItem : AchievementReward
    {
        public List<string> thingSetMakerTags;
        public List<ThingDef> additionalThingDefs;

        public override bool TryExecuteEvent()
        {
            try
            {
                List<ThingDef> rewards = DefDatabase<ThingDef>.AllDefs.Where(t => !t.thingSetMakerTags.NullOrEmpty() && t.thingSetMakerTags.Any(tag => thingSetMakerTags.Contains(tag))).ToList();
                if (!additionalThingDefs.NullOrEmpty())
                {
                    rewards.Concat(additionalThingDefs);
                }

                Rand.PushState();
                ThingDef randomReward = rewards.RandomElement();
                Rand.PopState();

                Thing reward = ThingMaker.MakeThing(randomReward);
                
                if (Find.CurrentMap != null)
                {
                    IntVec3 dropSpot = DropCellFinder.RandomDropSpot(Find.CurrentMap);
                    DropPodUtility.DropThingsNear(dropSpot, Find.CurrentMap, new List<Thing>() { reward });
                }
                else
                {
                    Map map = Find.Maps.FirstOrDefault(m => m.IsPlayerHome);
                    if (map is null)
                    {
                        Log.Error("Failed to find map to drop quest reward item. Canceling request.");
                        return false;
                    }
                    IntVec3 dropSpot = DropCellFinder.RandomDropSpot(map);
                    DropPodUtility.DropThingsNear(dropSpot, map, new List<Thing>() { reward });
                }
                Find.LetterStack.ReceiveLetter("RandomQuestRewardItemLetter".Translate(), "RandomQuestRewardItem".Translate(), LetterDefOf.PositiveEvent, reward);
            }
            catch(Exception ex)
            {
                Log.Error($"Failed to generate random quest reward item. Exception: {ex.Message}");
                return false;
            }
            return true;
        }
    }
}
