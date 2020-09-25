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
        protected override string[] DebugText => new string[] { $"Faction: {factionDef.defName}", $"Count: {count}"};

        public SettlementDefeatTracker()
        {
        }

        public SettlementDefeatTracker(SettlementDefeatTracker reference) : base(reference)
        {
            count = reference.count;
            factionDef = reference.factionDef;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref triggeredCount, "triggeredCount");
        }

        public override bool Trigger(Settlement settlement)
        {
            if (factionDef is null || factionDef == settlement.Faction.def)
            {
                triggeredCount++;
            }
            return triggeredCount >= count;
        }

        public int count;
        public FactionDef factionDef;

        protected int triggeredCount;
    }
}
