using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
	public class QuestTracker : Tracker2<Quest, QuestEndOutcome>
	{
		public QuestScriptDef def;
		public QuestEndOutcome outcome;
		public int count = 1;

		protected int triggeredCount;

		public override string Key => "QuestTracker";

		public override MethodInfo MethodHook => AccessTools.Method(typeof(Quest), nameof(Quest.End));
		public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.QuestEnded));
		protected override string[] DebugText => new string[] { $"Quest: {def?.defName}", $"Outcome: {outcome}", $"Count: {count}", $"Current: {triggeredCount}" };

		public QuestTracker()
		{
		}

		public QuestTracker(QuestTracker reference) : base(reference)
		{
			def = reference.def;
			outcome = reference.outcome;
			count = reference.count;
			triggeredCount = reference.triggeredCount;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref outcome, "outcome");
			Scribe_Values.Look(ref count, "count", 1);
			Scribe_Values.Look(ref triggeredCount, "triggeredCount");
		}

		public override (float percent, string text) PercentComplete => count > 1 ? ((float)triggeredCount / count, $"{triggeredCount} / {count}") : base.PercentComplete;

		public override bool Trigger(Quest quest, QuestEndOutcome outcome)
		{
			base.Trigger(quest, outcome);
			if (this.outcome == outcome && (def is null || (def == quest.root)))
			{
				triggeredCount++;
			}
			return triggeredCount >= count;
		}
	}
}
