using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AchievementsExpanded
{
	public class MultiRecordEventTracker : RecordEventTracker
	{
		public Dictionary<RecordDef, int> defs = new Dictionary<RecordDef, int>();

		protected override string[] DebugText
		{
			get
			{
				List<string> text = new List<string>();
				text.Add($"Defs (Count={defs.Count})");
				foreach (var def in defs)
				{
					string entry = $"Record: {def.Key?.defName ?? "None"} Count: {def.Value}";
					text.Add(entry);
				}
				return text.ToArray();
			}
		}

		public MultiRecordEventTracker()
		{
		}

		public MultiRecordEventTracker(MultiRecordEventTracker reference) : base(reference)
		{
			defs = reference.defs;
		}

		public override bool UnlockOnStartup
		{
			get
			{
				foreach (KeyValuePair<RecordDef, int> records in defs)
				{
					float value = 0;
					bool satisfied = false;
					foreach (Pawn pawn2 in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction)
					{
						value += pawn2.records.GetValue(records.Key);
						if (value >= records.Value)
						{
							if (total)
							{
								satisfied = true;
								break;
							}
							else
							{
								return true;
							}
						}
					}
					if (!satisfied)
						return false;
				}
				return false;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref defs, "defs", LookMode.Def, LookMode.Value);
		}

		public override bool Trigger(RecordDef record, Pawn pawn)
		{
			base.Trigger(record, pawn);
			if (!defs.ContainsKey(record))
			{
				return false;
			}
			if (total)
			{
				foreach (KeyValuePair<RecordDef, int> records in defs)
				{
					float value = 0;
					bool satisfied = false;
					foreach (Pawn pawn2 in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction)
					{
						value += pawn2.records.GetValue(records.Key);
						if (value >= records.Value)
						{
							satisfied = true;
							break;
						}
					}
					if (!satisfied)
						return false;
				}
				return true;
			}
			return defs.Any(d => pawn.records.GetValue(d.Key) >= d.Value);
		}
	}
}
