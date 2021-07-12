using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace AchievementsExpanded
{
	public class AchievementPointManager : GameComponent
	{
		internal const string AchievementTag = "[VanillaAchievementsExpanded]";

		public int availablePoints;
		public int totalEarnedPoints;

		internal static Dictionary<string, HashSet<AchievementCard>> achievementLookup = new Dictionary<string, HashSet<AchievementCard>>();
		internal static Dictionary<Type, string> typeToKey = new Dictionary<Type, string>();

		internal static HashSet<AchievementCard> tickerAchievements = new HashSet<AchievementCard>();

		internal HashSet<AchievementCard> achievementList;
		internal Stack<AchievementCard> unlockedCards;

		internal static HashSet<AchievementCard> AchievementList => Current.Game?.GetComponent<AchievementPointManager>()?.achievementList ?? new HashSet<AchievementCard>();
		internal static HashSet<Type> TrackerTypes { get; set; }
		internal static List<TrackerBase> TrackersGenerated { get; set; }

		public AchievementPointManager(Game game)
		{
		}

		public static void OnStartUp()
		{
			TrackerTypes = GenTypes.AllTypes.Where(t => t.IsSubclassOf(typeof(TrackerBase)) && !t.IsAbstract).ToHashSet();
			TrackersGenerated = new List<TrackerBase>();
			foreach (Type t in TrackerTypes)
			{
				var tracker = (TrackerBase)Activator.CreateInstance(t, null);
				TrackersGenerated.Add(tracker);
			}
		}

		public override void StartedNewGame()
		{
			base.StartedNewGame();
			PreInit();
		}

		public override void LoadedGame()
		{
			base.FinalizeInit();
			PreInit();
		}

		internal void HardReset()
		{
			achievementLookup = null;
			achievementList = null;
			unlockedCards = new Stack<AchievementCard>();
			DebugWriter.Log($"Regenerating achievements...\n");
			PreInit(true);
			ResetPoints();
		}

		private void PreInit(bool debug = false)
		{
			if (achievementLookup is null)
			{
				achievementLookup = new Dictionary<string, HashSet<AchievementCard>>();
			}
			if (achievementList is null)
			{
				achievementList = new HashSet<AchievementCard>();
			}
			if (unlockedCards is null)
			{
				unlockedCards = new Stack<AchievementCard>();
			}

			if (!UtilityMethods.BaseModActive)
			{
				return;
			}

			DebugWriter.ResetRootDir();
			if(debug)
				DebugWriter.Log($"Resetting AchievementLinks");
			achievementLookup = AchievementGenerator.GenerateAchievementLinks(achievementList);
			typeToKey.Clear();
			tickerAchievements.Clear();
			if(debug)
				DebugWriter.Log($"Verifying Achievement List");
			AchievementGenerator.VerifyAchievementList(ref achievementList, debug);

			CheckUnlocks();
		}

		private void CheckUnlocks()
		{
			foreach (AchievementCard card in achievementList)
			{
				if (card.tracker.UnlockOnStartup)
				{
					card.UnlockCard();
				}
			}
		}

		public void ResetAchievement(AchievementCard card)
		{
			achievementList.Remove(card);
			AchievementCard newCard = (AchievementCard)Activator.CreateInstance(card.def.achievementClass, new object[] { card.def, false });
			achievementList.Add(newCard);

			achievementLookup = AchievementGenerator.GenerateAchievementLinks(achievementList);
			typeToKey.Clear();
			tickerAchievements.Clear();
		}

		public void ResetPoints()
		{
			availablePoints = 0;
			totalEarnedPoints = 0;
		}

		public void DisplayCardWindow(AchievementCard card)
		{
			unlockedCards.Push(card);
		}

		public bool TryPurchasePoints(int points)
		{
			if (DebugSettings.godMode)
				return true;
			if (availablePoints < points)
				return false;
			availablePoints -= points;
			return true;
		}

		public void RefundPoints(int points)
		{
			availablePoints += points;
		}

		public void AddPoints(int points)
		{
			availablePoints += points;
			totalEarnedPoints += points;
		}

		public static HashSet<AchievementCard> GetCards<T>(bool locked = true)
		{
			if (achievementLookup.TryGetValue(GetTrackerKey<T>(), out var hashset))
			{
				if (locked)
					return hashset.Where(c => !c.unlocked).ToHashSet();
				return hashset;
			}
			achievementLookup.Add(GetTrackerKey<T>(), AchievementList.Where(c => c.tracker.Key == GetTrackerKey<T>()).ToHashSet());
			if (locked)
				return achievementLookup[GetTrackerKey<T>()].Where(c => !c.unlocked).ToHashSet();
			return achievementLookup[GetTrackerKey<T>()];
		}

		public static string GetTrackerKey<T>()
		{
			if (typeToKey.TryGetValue(typeof(T), out string key))
			{
				return key;
			}
			var check = typeof(T).IsAbstract ?
				TrackersGenerated.FirstOrDefault(t => t.GetType().IsSubclassOf(typeof(T)))
				: TrackersGenerated.FirstOrDefault(t => t.GetType() == typeof(T));
			typeToKey.Add(typeof(T), check.Key);
			return typeToKey[typeof(T)];
		}

		public static HashSet<AchievementCard> GetLongTickCards()
		{
			if (tickerAchievements.EnumerableNullOrEmpty())
			{
				tickerAchievements.AddRange(AchievementList.Where(a => a.tracker.AttachToLongTick != null && !a.unlocked));
			}
			return tickerAchievements;
		}

		public override void GameComponentTick()
		{
			if (unlockedCards.Any() && !Find.WindowStack.Windows.Any(w => w.GetType() == typeof(AchievementNotification)))
			{
				Find.WindowStack.Add(new AchievementNotification(unlockedCards.Pop()));
			}
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref availablePoints, "points");
			Scribe_Values.Look(ref totalEarnedPoints, "totalEarnedPoints");

			Scribe_Collections.Look(ref achievementList, "achievementList", LookMode.Deep);
		}
	}
}
