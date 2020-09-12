using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using System.Reflection;
using HarmonyLib;

namespace AchievementsExpanded
{
    public class AnimalBondedTracker : Tracker<PawnKindDef>
    {
        public override string Key => "AnimalBondedTracker";

        public override MethodInfo MethodHook => AccessTools.Method(typeof(RelationsUtility), nameof(RelationsUtility.TryDevelopBondRelation));
        public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.AnimalBondedEvent));

        public AnimalBondedTracker()
        {
        }

        public AnimalBondedTracker(AnimalBondedTracker reference) : base(reference)
        {
            kindDef = reference.kindDef;
            count = reference.count;
            triggeredCount = 0;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref kindDef, "kindDef");
            Scribe_Values.Look(ref count, "count");
            Scribe_Values.Look(ref triggeredCount, "triggeredCount");
        }

        public override bool Trigger(PawnKindDef param)
        {
            if (param == kindDef)
            {
                triggeredCount++;
            }
            return triggeredCount >= count;
        }

        public PawnKindDef kindDef;
        public int count;

        private int triggeredCount;
    }
}
