using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AchievementsExpanded
{
	public class HediffTracker : Tracker<Hediff>
	{
		public HediffDef def;
		public int count = 1;

		protected int triggeredCount;

		public override string Key => "HediffTracker";

		public override MethodInfo MethodHook => AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.AddHediff), new Type[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo), typeof(DamageWorker.DamageResult) });
		public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.HediffAdded));
		protected override string[] DebugText => new string[] { $"Def: {def.defName}", $"Count: {count}", $"Current: {triggeredCount}"};

		public HediffTracker()
		{
		}

		public HediffTracker(HediffTracker reference) : base(reference)
		{
			def = reference.def;
			count = reference.count;
			triggeredCount = reference.triggeredCount;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref count, "count", 1);
			Scribe_Values.Look(ref triggeredCount, "triggeredCount");
		}

		public override (float percent, string text) PercentComplete => count > 1 ? ((float)triggeredCount / count, $"{triggeredCount} / {count}") : base.PercentComplete;

		public override bool Trigger(Hediff hediff)
		{
			if (hediff?.pawn != null && hediff.pawn.Faction == Faction.OfPlayerSilentFail && (def is null || def == hediff.def))
			{
				triggeredCount++;
			}
			return triggeredCount >= count;
		}
	}
}
