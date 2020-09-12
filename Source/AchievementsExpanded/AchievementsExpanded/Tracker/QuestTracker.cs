using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
    public class QuestTracker : Tracker2<Quest, QuestEndOutcome>
    {
        public override string Key => "QuestTracker";

        public override MethodInfo MethodHook => AccessTools.Method(typeof(Quest), nameof(Quest.End));
        public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.QuestEnded));

        public QuestTracker()
        {
        }

        public QuestTracker(QuestTracker reference) : base(reference)
        {
            quest = reference.quest;
            outcome = reference.outcome;
            count = reference.count;
            triggeredCount = reference.triggeredCount;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref quest, "quest");
            Scribe_Values.Look(ref outcome, "outcome");
            Scribe_Values.Look(ref count, "count");
            Scribe_Values.Look(ref triggeredCount, "triggeredCount");
        }

        public override bool Trigger(Quest quest, QuestEndOutcome outcome)
        {
            if (this.outcome == outcome && (this.quest is null || (this.quest == quest.root)))
            {
                triggeredCount++;
            }
            return triggeredCount >= count;
        }

        public QuestScriptDef quest;
        public QuestEndOutcome outcome;
        public int count;

        private int triggeredCount;
    }
}
