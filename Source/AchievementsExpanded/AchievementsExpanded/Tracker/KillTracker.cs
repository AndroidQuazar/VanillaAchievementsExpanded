using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
    public class KillTracker : Tracker<Pawn>
    {
        public override string Key => "KillTracker";

        public override MethodInfo MethodHook => AccessTools.Method(typeof(Pawn), nameof(Pawn.Kill));
        public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.KillPawn));

        public KillTracker()
        {
        }

        public KillTracker(KillTracker reference) : base(reference)
        {
            kindDef = reference.kindDef;
            raceDef = reference.raceDef;
            factionDef = reference.factionDef;
            count = reference.count;
            triggeredCount = 0;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref kindDef, "kindDef");
            Scribe_Defs.Look(ref raceDef, "raceDef");
            Scribe_Defs.Look(ref factionDef, "factionDef");
            Scribe_Values.Look(ref count, "count", 1);
            Scribe_Values.Look(ref triggeredCount, "triggeredCount", 0);
        }

        public override bool Trigger(Pawn param)
        {
            bool kind = kindDef is null || param.kindDef == kindDef;
            bool race = raceDef is null || param.def == raceDef;
            bool faction = factionDef is null || param.Faction.def == factionDef;
            bool hitCount = count <= 1 ? true : ++triggeredCount >= count;
            if (kind && faction && hitCount)
            {
                return true;
            }
            return false;
        }

        public PawnKindDef kindDef;
        public ThingDef raceDef;
        public FactionDef factionDef;
        public int count = 1;

        private int triggeredCount;
    }
}
