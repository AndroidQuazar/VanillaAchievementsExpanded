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
	public class Reward_RandomQuest : AchievementReward
	{
		public override bool TryExecuteEvent()
		{
			try
			{
				Slate slate = new Slate();
				float points = StorytellerUtility.DefaultSiteThreatPointsNow();
				QuestScriptDef script = NaturalRandomQuestChooser.ChooseNaturalRandomQuest(points, Find.CurrentMap);
				SetPoints(script, slate, points);
				GenerateQuest(script, slate);
			}
			catch(Exception ex)
			{
				Log.Error($"Failed to generate random quest reward. Exception: {ex.Message}");
				return false;
			}
			return true;
		}

		private void SetPoints(QuestScriptDef script, Slate slate, float points)
		{
			if (script != null)
			{
				if (script.IsRootDecree)
				{
					slate.Set("asker", PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists.RandomElement());
				}
				if (script == QuestScriptDefOf.LongRangeMineralScannerLump)
				{
					slate.Set("targetMineableThing", ThingDefOf.Gold);
					slate.Set("targetMineable", ThingDefOf.MineableGold);
					slate.Set("worker", PawnsFinder.AllMaps_FreeColonists.FirstOrDefault());
				}
				slate.Set("points", points);
			}
		}

		private void GenerateQuest(QuestScriptDef script, Slate slate)
		{
			if (script.IsRootDecree)
			{
				Pawn pawn = slate.Get<Pawn>("asker");
				if (pawn.royalty.AllTitlesForReading.NullOrEmpty())
				{
					pawn.royalty.SetTitle(Faction.OfEmpire, RoyalTitleDefOf.Knight, grantRewards: false);
					Messages.Message("Dev: Gave " + RoyalTitleDefOf.Knight.label + " title to " + pawn.LabelCap, pawn, MessageTypeDefOf.NeutralEvent, historical: false);
				}
				Find.CurrentMap.StoryState.RecordDecreeFired(script);
			}
			else
			{
				Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(script, slate);
				if (!quest.hidden)
				{
					QuestUtility.SendLetterQuestAvailable(quest);
				}
			}
		}
	}
}
