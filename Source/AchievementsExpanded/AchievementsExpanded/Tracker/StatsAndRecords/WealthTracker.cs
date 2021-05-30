using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
	public class WealthTracker : TrackerBase
	{
		public override string Key => "WealthTracker";

		public override Func<bool> AttachToLongTick => () => { return Trigger();  };
		protected override string[] DebugText => new string[] { $"Wealth: {count}" };
		public WealthTracker()
		{
		}

		public WealthTracker(WealthTracker reference) : base(reference)
		{
			count = reference.count;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref count, "count", 1);
		}

		public override bool Trigger()
		{
			base.Trigger();
			foreach (Map map in Find.Maps.Where(m => m.IsPlayerHome))
			{
				if (map.wealthWatcher.WealthTotal >= count)
				{
					return true;
				}
			}
			return false;
		}

		public override bool UnlockOnStartup => Trigger();

		public int count = 1;
	}
}
