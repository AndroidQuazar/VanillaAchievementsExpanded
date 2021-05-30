using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using System.Reflection;
using HarmonyLib;

namespace AchievementsExpanded
{
	public class AnimalBondedTracker : Tracker<PawnKindDef>
	{
		public PawnKindDef kindDef;
		public int count = 1;

		protected int triggeredCount;

		public override string Key => "AnimalBondedTracker";

		public override MethodInfo MethodHook => AccessTools.Method(typeof(RelationsUtility), nameof(RelationsUtility.TryDevelopBondRelation));
		public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.AnimalBondedEvent));
		protected override string[] DebugText => new string[] { $"KindDef: {kindDef?.defName ?? "None"}", $"Count: {count}", $"Current: {triggeredCount}" };

		public AnimalBondedTracker()
		{
		}

		public AnimalBondedTracker(AnimalBondedTracker reference) : base(reference)
		{
			kindDef = reference.kindDef;
			count = reference.count;
			triggeredCount = 0;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref kindDef, "kindDef");
			Scribe_Values.Look(ref count, "count", 1);
			Scribe_Values.Look(ref triggeredCount, "triggeredCount");
		}

		public override (float percent, string text) PercentComplete => count > 1 ? ((float)triggeredCount / count, $"{triggeredCount} / {count}") : base.PercentComplete;

		public override bool Trigger(PawnKindDef param)
		{
			base.Trigger(param);
			if (kindDef is null || param == kindDef)
			{
				triggeredCount++;
			}
			return triggeredCount >= count;
		}
	}
}
