using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
	public class MultiHediffTracker : HediffTracker
	{
		public Dictionary<HediffDef, int> defs = new Dictionary<HediffDef, int>();

		public MultiHediffTracker()
		{
		}

		public MultiHediffTracker(MultiHediffTracker reference) : base(reference)
		{
			defs = reference.defs;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref defs, "defs", LookMode.Def, LookMode.Value);
		}

		public override bool Trigger(Hediff hediff)
		{
			base.Trigger(hediff);
			if (defs.ContainsKey(hediff.def) && hediff.pawn.Faction == Faction.OfPlayerSilentFail)
			{
				var hediffCounts = new Dictionary<HediffDef, int>(defs);
				foreach (Hediff curHediff in hediff.pawn.health.hediffSet.hediffs)
				{
					if (hediffCounts.TryGetValue(curHediff.def, out int _))
					{
						hediffCounts[curHediff.def]--;
					}
				}
				if (hediffCounts.Values.All(c => c <= 0))
				{
					return true;
				}
			}
			return false;
		}
	}
}
