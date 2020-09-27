using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace AchievementsExpanded
{
	/// <summary>
	/// Easier and prettier than implementing IndexOf directly into CardList filter
	/// </summary>
	public static class UtilityMethods
	{
		/// <summary>
		/// Check if string contains another string within
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <param name="comp"></param>
		/// <returns></returns>
		public static bool Contains(this string source, string target, StringComparison comp)
		{
			return source?.IndexOf(target, comp) >= 0;
		}

		/// <summary>
		/// Check if Type is equal to or subclass from target Type
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool SameOrSubclass(this Type source, Type target)
		{
			return source == target || source.IsSubclassOf(target);
		}

		/// <summary>
		/// Helper Method for MentalBreak Transpiler (Check if MentalBreak Initiated -> Trigger on Trackers)
		/// </summary>
		public static void MentalBreakTrigger(bool started, MentalBreakDef def)
		{
			foreach (var card in AchievementPointManager.AchievementList.Where(a => a.tracker.GetType().SameOrSubclass(typeof(MentalBreakTracker)) && !a.unlocked))
			{
				if (started && (card.tracker as MentalBreakTracker).Trigger(def))
				{
					card.UnlockCard();
				}
			}
		}

		/// <summary>
		/// Helper Method for UnfinishedThing crafted Transpiler
		/// </summary>
		/// <param name="thing"></param>
		public static void ItemCraftedTrigger(Thing thing)
        {
			foreach(var card in AchievementPointManager.AchievementList.Where(a => a.tracker.GetType().SameOrSubclass(typeof(ItemCraftTracker)) && !a.unlocked))
            {
                if ((card.tracker as ItemCraftTracker).Trigger(thing))
                {
                    card.UnlockCard();
                }
            }
        }

		/// <summary>
		/// HelperMethod for LevelUp Mote Transpiler
		/// </summary>
		/// <param name="skill"></param>
		public static void LevelUpTrigger(SkillDef skill, int level)
        {
			foreach(var card in AchievementPointManager.AchievementList.Where(a => a.tracker.GetType().SameOrSubclass(typeof(SkillTracker)) && !a.unlocked))
            {
                if ((card.tracker as SkillTracker).Trigger(skill, level))
                {
                    card.UnlockCard();
                }
            }
        }

		/// <summary>
		/// HelperMethod for Time based record event
		/// </summary>
		/// <param name="def"></param>
		/// <param name="pawn"></param>
		/// <param name="interval"></param>
		public static void RecordTimeEvent(RecordDef def, Pawn pawn, int interval)
        {
			foreach (var card in AchievementPointManager.AchievementList.Where(a => a.tracker.GetType().SameOrSubclass(typeof(RecordTimeTracker)) && !a.unlocked))
            {
                var tracker = card.tracker as RecordTimeTracker;
                if (tracker.def == def && tracker.Trigger(def, pawn, (float)interval))
                {
                    card.UnlockCard();
                }
            }
        }

		/// <summary>
		/// Get all items from Player's home inventory
		/// </summary>
		/// <param name="thingDef"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static bool PlayerHas(ThingDef thingDef, out int total, int count = 1)
		{
			if (count <= 0)
			{
				total = 0;
				return true;
			}
			int num = 0;
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				num += maps[i].listerThings.ThingsOfDef(thingDef).Sum(t => t.stackCount);
				if (num >= count)
				{
					total = num;
					return true;
				}
			}
			total = num;
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int j = 0; j < caravans.Count; j++)
			{
				if (caravans[j].IsPlayerControlled)
				{
					List<Thing> list = CaravanInventoryUtility.AllInventoryItems(caravans[j]);
					for (int k = 0; k < list.Count; k++)
					{
						if (list[k].def == thingDef)
						{
							num += list[k].stackCount;
							total += num;
							if (num >= count)
							{
								return true;
							}
						}
					}
				}
			}
			return total >= count;
		}
	}
}
