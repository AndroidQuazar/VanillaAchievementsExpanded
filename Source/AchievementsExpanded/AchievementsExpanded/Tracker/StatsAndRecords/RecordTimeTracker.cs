using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AchievementsExpanded
{
	public class RecordTimeTracker : Tracker3<RecordDef, Pawn, float>
	{
		public RecordDef def;
		public float count = 1;
		public bool total;

		public override string Key => "RecordTimeTracker";
		public override MethodInfo MethodHook => AccessTools.Method(typeof(Pawn_RecordsTracker), "RecordsTickUpdate");
		public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.RecordTimeEvent));
		public override PatchType PatchType => PatchType.Transpiler;
		protected override string[] DebugText => new string[] { $"Def: {def.defName}", $"Count: {count}", $"Total: {total}" };

		public RecordTimeTracker()
		{
		}

		public RecordTimeTracker(RecordTimeTracker reference) : base(reference)
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
					return Trigger(def, null, 0);
				}
				foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction)
				{
					if (Trigger(def, pawn, 0))
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

		public override bool Trigger(RecordDef record, Pawn pawn, float next)
		{
			base.Trigger(record, pawn);
			if (pawn?.Faction != Faction.OfPlayerSilentFail)
			{
				return false;
			}
			if (total)
			{
				float value = next;
				foreach (Map map in Find.Maps)
				{
					foreach (Pawn pawn2 in map.mapPawns.FreeColonists)
					{
						value += pawn2.records.GetValue(record);
						if (value >= count)
							return true;
					}
					if (value >= count)
							return true;
				}
				return false;
			}
			return (next + pawn.records.GetValue(def)) >= count;
		}
	}
}
