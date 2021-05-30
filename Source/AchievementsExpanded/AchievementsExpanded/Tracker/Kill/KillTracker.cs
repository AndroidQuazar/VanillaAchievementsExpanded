using System;
using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;

namespace AchievementsExpanded
{
	public class KillTracker : Tracker2<Pawn, DamageInfo?>
	{
		public PawnKindDef kindDef;
		public ThingDef raceDef;
		public List<FactionDef> factionDefs;
		public List<FactionDef> instigatorFactionDefs;
		public int count = 1;

		protected int triggeredCount;
		protected List<string> killedThings;

		public override string Key => "KillTracker";

		public override MethodInfo MethodHook => AccessTools.Method(typeof(Pawn), nameof(Pawn.Kill));
		public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.KillPawn));
		protected override string[] DebugText => new string[] { $"KindDef: {kindDef?.defName ?? "None"}", 
																$"Race: {raceDef?.defName ?? "None"}", 
																$"Factions: {factionDefs?.Count.ToString() ?? "None"}",
																$"Instigators: {instigatorFactionDefs?.Count.ToString() ?? "None"}",
																$"Count: {count}", $"Current: {triggeredCount}" };
		public override PatchType PatchType => PatchType.Prefix;

		public KillTracker()
		{
		}

		public KillTracker(KillTracker reference) : base(reference)
		{
			kindDef = reference.kindDef;
			raceDef = reference.raceDef;
			factionDefs = reference.factionDefs;
			instigatorFactionDefs = reference.instigatorFactionDefs;
			count = reference.count;
			triggeredCount = 0;

			killedThings = new List<string>();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref kindDef, "kindDef");
			Scribe_Defs.Look(ref raceDef, "raceDef");
			Scribe_Collections.Look(ref factionDefs, "factionDefs", LookMode.Def);
			Scribe_Collections.Look(ref instigatorFactionDefs, "instigatorFactionDefs", LookMode.Def);
			Scribe_Values.Look(ref count, "count", 1);

			Scribe_Values.Look(ref triggeredCount, "triggeredCount", 0);
			Scribe_Collections.Look(ref killedThings, "killedThings", LookMode.Value);
		}

		public override (float percent, string text) PercentComplete => count > 1 ? ((float)triggeredCount / count, $"{triggeredCount} / {count}") : base.PercentComplete;

		public override bool Trigger(Pawn pawn, DamageInfo? dinfo)
		{
			base.Trigger(pawn, dinfo);
			if (killedThings.Contains(pawn.GetUniqueLoadID()))
				return false;
			else
				killedThings.Add(pawn.GetUniqueLoadID());
			bool instigator = instigatorFactionDefs.NullOrEmpty() || (dinfo?.Instigator?.Faction?.def != null && instigatorFactionDefs.Contains(dinfo.Value.Instigator.Faction.def));
			bool kind = kindDef is null || pawn.kindDef == kindDef;
			bool race = raceDef is null || pawn.def == raceDef;
			bool faction = factionDefs.NullOrEmpty() || (pawn.Faction != null && factionDefs.Contains(pawn.Faction.def));
			return kind && race && faction && instigator && (count <= 1 || ++triggeredCount >= count);
		}
	}
}
