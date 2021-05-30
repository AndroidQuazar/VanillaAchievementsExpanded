using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using HarmonyLib;

namespace AchievementsExpanded
{
	public abstract class PawnJoinedTracker : Tracker<Pawn>
	{
		public bool requireAll = true;

		public override string Key => "PawnJoinedTracker";

		public override MethodInfo MethodHook => AccessTools.Method(typeof(StoryWatcher_PopAdaptation), nameof(StoryWatcher_PopAdaptation.Notify_PawnEvent));
		public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.PawnJoinedFaction));

		public PawnJoinedTracker()
		{
		}

		public PawnJoinedTracker(PawnJoinedTracker reference) : base(reference)
		{
			requireAll = reference.requireAll;
		}

		public override bool UnlockOnStartup => Trigger(null);

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref requireAll, "requireAll", true);
		}
	}
}
