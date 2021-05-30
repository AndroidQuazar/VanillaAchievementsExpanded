using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace AchievementsExpanded
{
	public class SettlementDefeatTracker : Tracker<Settlement>
	{
		public FactionDef def;
		public int count = 1;

		protected int triggeredCount;

		public override string Key => "SettlementDefeatTracker";

		public override MethodInfo MethodHook => AccessTools.Method(typeof(SettlementDefeatUtility), "IsDefeated");
		public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.SettlementDefeatedEvent));
		protected override string[] DebugText => new string[] { $"Faction: {def?.defName ?? "[NullDef]"}", $"Count: {count}"};

		public SettlementDefeatTracker()
		{
		}

		public SettlementDefeatTracker(SettlementDefeatTracker reference) : base(reference)
		{
			def = reference.def;
			count = reference.count;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref count, "count", 1);
			Scribe_Values.Look(ref triggeredCount, "triggeredCount");
		}

		public override (float percent, string text) PercentComplete => count > 1 ? ((float)triggeredCount / count, $"{triggeredCount} / {count}") : base.PercentComplete;

		public override bool Trigger(Settlement settlement)
		{
			base.Trigger(settlement);
			if (def is null || def == settlement.Faction.def)
			{
				triggeredCount++;
			}
			return triggeredCount >= count;
		}
	}
}
