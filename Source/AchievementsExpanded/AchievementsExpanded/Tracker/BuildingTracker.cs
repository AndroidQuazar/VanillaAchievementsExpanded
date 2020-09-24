using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
    public class BuildingTracker : Tracker2<BuildableDef, ThingDef>
    {
        public override string Key => "BuildingTracker";

        public override MethodInfo MethodHook => AccessTools.Method(typeof(Frame), nameof(Frame.CompleteConstruction));
        public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.BuildingSpawned));
        protected override string[] DebugText => new string[] { $"Def: {def?.defName ?? "None"}", $"MadeFrom: {madeFrom?.defName ?? "Any"}", $"Count: {count}", $"Current: {triggeredCount}" };
        public BuildingTracker()
        {
        }

        public BuildingTracker(BuildingTracker reference) : base(reference)
        {
            def = reference.def;
            madeFrom = reference.madeFrom;
            count = reference.count;
            triggeredCount = 0;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref def, "def");
            Scribe_Defs.Look(ref madeFrom, "madeFrom");
            Scribe_Values.Look(ref count, "count", 1);
            Scribe_Values.Look(ref triggeredCount, "triggeredCount");
        }

        public override bool Trigger(BuildableDef building, ThingDef stuff)
        {
            base.Trigger(building, stuff);
            if ( (def is null || def == building) && (madeFrom is null || madeFrom == stuff) )
            {
                triggeredCount++;
            }
            return triggeredCount >= count;
        }

        public ThingDef def;
        public ThingDef madeFrom;
        public int count = 1;

        private int triggeredCount;
    }
}
