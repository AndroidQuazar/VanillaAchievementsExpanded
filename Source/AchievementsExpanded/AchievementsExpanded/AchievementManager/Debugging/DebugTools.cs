using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Verse;
using RimWorld;
using HarmonyLib;

namespace AchievementsExpanded
{
	public static class DebugTools
	{
		internal const string VAEDebugCategory = "Vanilla Achievements Expanded";

		[AchievementDebugAction(category = VAEDebugCategory)]
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
						card.UnlockCard(true);
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			}
			else
			{
				Messages.Message("No Achievements To Unlock", MessageTypeDefOf.RejectInput);
			}
		}

		[AchievementDebugAction(VAEDebugCategory)]
		private static void LockAchievement()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			var lockedAchievements = AchievementPointManager.AchievementList.Where(c => !c.unlocked);
			if (!lockedAchievements.EnumerableNullOrEmpty())
			{
				foreach (AchievementCard card in lockedAchievements.OrderBy(a => a.def.defName))
				{
					list.Add(new DebugMenuOption(card.def.defName, DebugMenuOptionMode.Action, delegate()
					{
						card.LockCard();
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			}
			else
			{
				Messages.Message("No Achievements To Lock", MessageTypeDefOf.RejectInput);
			}
		}

		[AchievementDebugAction(VAEDebugCategory)]
		private static void RegenerateAchievement()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			var lockedAchievements = AchievementPointManager.AchievementList;
			if (!lockedAchievements.EnumerableNullOrEmpty())
			{
				foreach (AchievementCard card in lockedAchievements.OrderBy(a => a.def.defName))
				{
					list.Add(new DebugMenuOption(card.def.defName, DebugMenuOptionMode.Action, delegate()
					{
						Current.Game?.GetComponent<AchievementPointManager>().ResetAchievement(card);
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			}
			else
			{
				Messages.Message("No Achievements To Unlock", MessageTypeDefOf.RejectInput);
			}
		}

		[AchievementDebugAction(VAEDebugCategory)]
		private static void UnlockAllAchievements()
		{
			foreach (AchievementCard card in AchievementPointManager.AchievementList)
			{
				card.UnlockCard(true);
			}
		}

		[AchievementDebugAction(VAEDebugCategory)]
		private static void LockAllAchievements()
		{
			foreach (AchievementCard card in AchievementPointManager.AchievementList)
			{
				card.LockCard();
			}

			Current.Game.GetComponent<AchievementPointManager>().ResetPoints();
		}

		[AchievementDebugAction(VAEDebugCategory)]
		private static void RegenerateAllAchievements()
		{
			Current.Game.GetComponent<AchievementPointManager>().HardReset();
		}

		[AchievementDebugAction(VAEDebugCategory)]
		private static void OutputAchievementList()
		{
			DebugWriter.Log("---- ACHIEVEMENT CARD LIST ----");
			Log.Message("---- ACHIEVEMENT CARD LIST ----");
			foreach (AchievementCard card in AchievementPointManager.AchievementList)
			{
				string text = string.Concat(new object[]
				{
					"Card: ",
					card.def.label + "\n",
					"Tracker: ",
					card.tracker + "\n"
				});
				DebugWriter.Log(text);
				Log.Message(text);
			}
			DebugWriter.Log("--------------------------");
			Log.Message("--------------------------");
		}

		[AchievementDebugAction(VAEDebugCategory)]
		private static void OutputAchievementKey()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			var keys = AchievementPointManager.AchievementList.Select(a => a.tracker.Key).ToHashSet();
			if (!keys.EnumerableNullOrEmpty())
			{
				foreach (string key in keys)
				{
					list.Add(new DebugMenuOption(key, DebugMenuOptionMode.Action, delegate()
					{
						DebugWriter.Log($"Outputting Achievements with key: {key}");
						Log.Message($"Outputting Achievements with key: {key}");
						foreach (var card in AchievementPointManager.AchievementList.Where(a => a.tracker.Key == key))
						{
							DebugWriter.Log(card.def.label);
							Log.Message(card.def.label);
						}
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			}
			else
			{
				Messages.Message("No AchievementKeys To Check", MessageTypeDefOf.RejectInput);
			}
		}

		[AchievementDebugAction(VAEDebugCategory)]
		private static void OutputAchievementTickerCards()
		{
			DebugWriter.Log("---- ACHIEVEMENT TICKER CARDS ----");
			Log.Message("---- ACHIEVEMENT TICKER CARDS ----");
			foreach (AchievementCard card in AchievementPointManager.tickerAchievements)
			{
				string text = string.Concat(new object[]
				{
					"Card: ",
					card.def.label + "\n",
					"Tracker: ",
					card.tracker + "\n"
				});
				DebugWriter.Log(text);
				Log.Message(text);
			}
			DebugWriter.Log("--------------------------");
			Log.Message("--------------------------");
		}

		[AchievementDebugAction(VAEDebugCategory)]
		private static void WriteToFile()
		{
			DebugWriter.PushToFile();
		}
	}
}
