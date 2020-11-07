using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace AchievementsExpanded
{
    public class SettlementDefeatTracker : Tracker<Settlement>
    {
        public override string Key => "SettlementDefeatTracker";

        public override MethodInfo MethodHook => AccessTools.Method(typeof(SettlementDefeatUtility), "IsDefeated");
        public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.SettlementDefeatedEvent));
        protected override string[] DebugText => new string[] { $"Faction: {def.defName}", $"Count: {count}"};

        public SettlementDefeatTracker()
        {
        }

        public SettlementDefeatTracker(SettlementDefeatTracker reference) : base(reference)
        {
            def = reference.def;
            count = reference.count;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref count, "count", 1);
            Scribe_Values.Look(ref triggeredCount, "triggeredCount");
        }

        public override bool Trigger(Settlement settlement)
        {
            if (def is null || def == settlement.Faction.def)
            {
                triggeredCount++;
            }
            return triggeredCount >= count;
        }

        public FactionDef def;
        public int count = 1;

        protected int triggeredCount;
    }
}
