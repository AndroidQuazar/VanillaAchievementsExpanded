using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace AchievementsExpanded
{
    public class MentalBreakTracker : Tracker<MentalBreakDef>
    {
        public override string Key => "MentalBreakTracker";

        public override MethodInfo MethodHook => AccessTools.Method(typeof(MentalBreaker), nameof(MentalBreaker.TryDoRandomMoodCausedMentalBreak));
        public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.MentalBreakTriggered));
        public override PatchType PatchType => PatchType.Transpiler;

        public MentalBreakTracker()
        {
        }

        public MentalBreakTracker(MentalBreakTracker reference) : base(reference)
        {
            def = reference.def;
            count = reference.count;
            triggeredCount = 0;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref count, "count");
            Scribe_Values.Look(ref triggeredCount, "triggeredCount", 0);
        }

        public override bool Trigger(MentalBreakDef def)
        {
            if(this.def == def)
            {
                triggeredCount++;
                return triggeredCount >= count;
            }
            return false;
        }

        public MentalBreakDef def;
        public int count;

        private int triggeredCount;
    }
}
