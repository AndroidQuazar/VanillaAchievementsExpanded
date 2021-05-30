using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using HarmonyLib;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
	public class ItemTracker : TrackerBase
	{
		public ThingDef def;
		public int count = 1;

		[Unsaved]
		protected int triggeredCount; //Only for display

		public override string Key => "ItemTracker";

		public override Func<bool> AttachToLongTick => () => { return Trigger(); };
		protected override string[] DebugText => new string[] { $"Def: {def?.defName ?? "None"}", $"Count: {count}" };

		public ItemTracker()
		{
		}

		public ItemTracker(ItemTracker reference) : base(reference)
		{
			def = reference.def;
			count = reference.count;
		}

		public override bool UnlockOnStartup => Trigger();

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref count, "count", 1);
		}

		public override (float percent, string text) PercentComplete => count > 1 ? ((float)triggeredCount / count, $"{triggeredCount} / {count}") : base.PercentComplete;

		public override bool Trigger()
		{
			base.Trigger();
			return UtilityMethods.PlayerHas(def, out triggeredCount, count);
		}
	}
}
