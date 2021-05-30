using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AchievementsExpanded
{
	public class HediffDeathTracker : Tracker<Hediff>
	{
		public HediffDef def;
		public int count = 1;

		protected int triggeredCount;

		public override string Key => "HediffDeathTracker";

		public override MethodInfo MethodHook => AccessTools.Method(typeof(Hediff), nameof(Hediff.CauseDeathNow));
		public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.HediffDeathEvent));
		protected override string[] DebugText => new string[] { $"Def: {def.defName}"};

		public HediffDeathTracker()
		{
		}

		public HediffDeathTracker(HediffDeathTracker reference) : base(reference)
		{
			def = reference.def;
			count = reference.count;
			triggeredCount = reference.triggeredCount;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref count, "count");

			Scribe_Values.Look(ref triggeredCount, "triggeredCount");
		}

		public override (float percent, string text) PercentComplete => count > 1 ? ((float)triggeredCount / count, $"{triggeredCount} / {count}") : base.PercentComplete;

		public override bool Trigger(Hediff hediff)
		{
			if (hediff.pawn != null && (hediff.pawn.Faction == Faction.OfPlayer || hediff.pawn.IsPrisonerOfColony) && (def is null || hediff.def == def))
			{
				triggeredCount++;
			}
			return triggeredCount >= count;
		}
	}
}
