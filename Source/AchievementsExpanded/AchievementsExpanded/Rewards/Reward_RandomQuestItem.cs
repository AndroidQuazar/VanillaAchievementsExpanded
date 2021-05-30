using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using RimWorld.Planet;
using Verse;
using UnityEngine;

namespace AchievementsExpanded
{
	public class Reward_RandomQuestItem : AchievementReward
	{
		public List<string> thingSetMakerTags;
		public List<ThingDef> additionalThingDefs;

		public override string Disabled
		{
			get
			{
				string reason = base.Disabled;
				if (Find.CurrentMap is null)
				{
					reason += "\n" + "NoValidMap".Translate();
				}
				return reason;
			}
		}

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

				Thing reward = ThingMaker.MakeThing(randomReward);
				int stackCount = 1;
				if (reward is Building building)
				{
					reward = building.MakeMinified();
				}
				else if (reward.def.stackLimit > 1)
				{
					int stackLimit = Mathf.Clamp(reward.def.stackLimit, 1, 500);
					stackCount = Rand.Range(stackLimit / 4, stackLimit);
					float itemValue = reward.def.BaseMarketValue;
					if (itemValue >= 2000)
					{
						stackCount = 1;
					}
					else if (itemValue >= 1500)
					{
						stackCount /= 30;
					}
					else if (itemValue >= 1000)
					{
						stackCount /= 20;
					}
					else if (itemValue >= 500)
					{
						stackCount /= 10;
					}
					else if (itemValue >= 50)
					{
						stackCount /= 2;
					}
				}
				reward.stackCount = stackCount;
				Rand.PopState();
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
