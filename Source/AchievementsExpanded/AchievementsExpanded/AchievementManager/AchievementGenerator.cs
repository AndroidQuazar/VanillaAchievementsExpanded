using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
	public static class AchievementGenerator
	{
		public static Dictionary<string, HashSet<AchievementCard>> GenerateAchievementLinks(HashSet<AchievementCard> cards)
		{
			var lookup = new Dictionary<string, HashSet<AchievementCard>>();

			DebugWriter.Log("Generating Achievement Links...");
			AchievementCard currentCard = null;
			try
			{
				foreach (AchievementCard card in cards)
				{
					currentCard = card;
					DebugWriter.Log($"Linking card {card.def.label} to {card.tracker.Key}");
					if (lookup.TryGetValue(card.tracker.Key, out var hash))
					{
						hash.Add(card);
					}
					else
					{
						lookup.Add(card.tracker.Key, new HashSet<AchievementCard>() { card });
					}
				}
			}
			catch (Exception ex)
			{
				string error = $"Failed to generate Achievement Links for {currentCard?.def.label ?? "[Null Card]"}. Exception: {ex.Message}";
				Log.Error(error);
				DebugWriter.Log(error);
				return new Dictionary<string, HashSet<AchievementCard>>();
			}
			return lookup;
		}

		public static bool VerifyAchievementList(ref HashSet<AchievementCard> achievementCards, bool debugOutput = false)
		{
			bool newlyAdded = false;
			int count = 0;
			int defCount = 0;
			foreach (AchievementDef def in DefDatabase<AchievementDef>.AllDefs)
			{
				var card = achievementCards.FirstOrDefault(a => a.def.defName == def.defName);
				if (def.achievementClass is null)
					def.achievementClass = typeof(AchievementCard);
				if (card is null)
				{
					card = (AchievementCard)Activator.CreateInstance(def.achievementClass, new object[] { def, false});
					achievementCards.Add(card);
					newlyAdded = true;
					count++;
				}
				else if (card.tracker is null || card.def is null)
				{
					Log.Warning($"{AchievementPointManager.AchievementTag} Corrupted AchievementCard detected. " +
						$"Regenerating {card?.GetUniqueLoadID() ?? "[Null Card]"}. Your current progress for it will be lost but it will remain unlocked if already completed.\n " +
						$"If the problem persists, consider manually resetting {card.def?.defName ?? "[Null Def]"} through the DebugTools or reporting on the Steam Workshop.");
					achievementCards.Remove(card);
					var card2 = (AchievementCard)Activator.CreateInstance(def.achievementClass, new object[] { def, card.unlocked });
					achievementCards.Add(card2);
					newlyAdded = true;
					count++;
				}
				defCount++;
			}
			if(debugOutput)
				Log.Message($"{AchievementPointManager.AchievementTag} {count}/{defCount} achievements generated.");
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
