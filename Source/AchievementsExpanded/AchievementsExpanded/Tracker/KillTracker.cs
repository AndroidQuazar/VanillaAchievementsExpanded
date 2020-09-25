using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;
using System.Collections.Generic;

namespace AchievementsExpanded
{
    public class KillTracker : Tracker<Pawn>
    {
        public override string Key => "KillTracker";

        public override MethodInfo MethodHook => AccessTools.Method(typeof(Pawn), nameof(Pawn.Kill));
        public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.KillPawn));
        protected override string[] DebugText => new string[] { $"KindDef: {kindDef?.defName ?? "None"}", 
                                                                $"Race: {raceDef?.defName ?? "None"}", 
                                                                $"Factions: {factionDefs?.Count.ToString() ?? "None"}", 
                                                                $"Count: {count}", $"Current: {triggeredCount}" };
        public KillTracker()
        {
        }

        public KillTracker(KillTracker reference) : base(reference)
        {
            kindDef = reference.kindDef;
            raceDef = reference.raceDef;
            factionDefs = reference.factionDefs;
            count = reference.count;
            triggeredCount = 0;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref kindDef, "kindDef");
            Scribe_Defs.Look(ref raceDef, "raceDef");
            Scribe_Collections.Look(ref factionDefs, "factionDefs", LookMode.Def);
            Scribe_Values.Look(ref count, "count", 1);
            Scribe_Values.Look(ref triggeredCount, "triggeredCount", 0);
        }

        public override bool Trigger(Pawn pawn)
        {
            base.Trigger(pawn);
            bool kind = kindDef is null || pawn.kindDef == kindDef;
            bool race = raceDef is null || pawn.def == raceDef;
            bool faction = factionDefs.NullOrEmpty() || factionDefs.Contains(pawn.Faction.def);
            bool hitCount = count <= 1 ? true : ++triggeredCount >= count;
            if (kind && race && faction && hitCount)
            {
                return true;
            }
            return false;
        }

        public PawnKindDef kindDef;
        public ThingDef raceDef;
        public List<FactionDef> factionDefs;
        public int count = 1;

        protected int triggeredCount;
    }
}
