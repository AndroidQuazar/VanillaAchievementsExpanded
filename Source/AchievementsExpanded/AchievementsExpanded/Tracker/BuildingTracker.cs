using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
    public class BuildingTracker : Tracker<Building>
    {
        public override string Key => "BuildingTracker";

        public override MethodInfo MethodHook => AccessTools.Method(typeof(GenSpawn), nameof(GenSpawn.Spawn), new Type[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool) });
        public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.ThingBuildingSpawned));
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

        public override bool Trigger(Building building)
        {
            base.Trigger(building);
            if (building.Faction ==  Faction.OfPlayer && (def is null || def == building.def) && (madeFrom is null || madeFrom == building.Stuff) )
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
