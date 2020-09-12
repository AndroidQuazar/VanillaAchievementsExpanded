using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using HarmonyLib;

namespace AchievementsExpanded
{
    public class IncidentTracker : Tracker2<IncidentDef, Map>
    {
        public override string Key => "IncidentTracker";

        public override MethodInfo MethodHook => AccessTools.Method(typeof(IncidentWorker), nameof(IncidentWorker.TryExecute));
        public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.IncidentTriggered));

        public IncidentTracker()
        {
        }

        public IncidentTracker(IncidentTracker reference) : base(reference)
        {
            def = reference.def;
            count = reference.count;
            triggeredCount = 0;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref triggeredCount, "triggeredCount", 0);
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref count, "count");
        }

        public override bool Trigger(IncidentDef param1, Map map)
        {
            if (param1 == def)
            {
                triggeredCount++;
                if (triggeredCount >= count)
                {
                    return true;
                }
            }
            return false;
        }

        public IncidentDef def;
        public int count;

        public int triggeredCount;
    }
}
