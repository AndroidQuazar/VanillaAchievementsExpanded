using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AchievementsExpanded
{
	public class RecordEventTracker : Tracker2<RecordDef, Pawn>
	{
		public RecordDef def;
		public float count = 1;
		public bool total;

		[Unsaved]
		protected float triggeredCount;

		public override string Key => "RecordEventTracker";
		public override MethodInfo MethodHook => AccessTools.Method(typeof(Pawn_RecordsTracker), nameof(Pawn_RecordsTracker.Increment)); //Patch on AddTo as well
		public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.RecordEvent));
		protected override string[] DebugText => new string[] { $"Def: {def?.defName ?? "[NullDef]"}", $"Count: {count}" };

		public RecordEventTracker()
		{
		}

		public RecordEventTracker(RecordEventTracker reference) : base(reference)
		{
			def = reference.def;
			count = reference.count;
			total = reference.total;
		}

		public override bool UnlockOnStartup
		{
			get
			{
				if (total)
				{
					return Trigger(def, null);
				}
				foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction)
				{
					if (Trigger(def, pawn))
					{
						return true;
					}
				}
				return false;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref count, "count", 1);
			Scribe_Values.Look(ref total, "total");
		}

		public override (float percent, string text) PercentComplete => total && count > 1 ? (triggeredCount / count, $"{triggeredCount} / {count}") : base.PercentComplete;

		public override bool Trigger(RecordDef record, Pawn pawn)
		{
			base.Trigger(record, pawn);
			if (def != record)
			{
				return false;
			}
			if (pawn?.Faction != Faction.OfPlayerSilentFail)
			{
				return false;
			}
			if (total)
			{
				triggeredCount = 0;
				foreach (Pawn pawn2 in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction)
				{
					triggeredCount += pawn2.records.GetValue(record);
					if (triggeredCount >= count)
						return true;
				}
				if (triggeredCount >= count)
						return true;
				return false;
			}
			return pawn.records.GetValue(def) >= count;
		}
	}
}
