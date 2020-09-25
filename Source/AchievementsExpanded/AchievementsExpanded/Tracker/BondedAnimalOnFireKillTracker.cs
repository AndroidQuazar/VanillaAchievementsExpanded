using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AchievementsExpanded
{
    public class BondedAnimalOnFireKillTracker : KillTracker
    {
        public override string Key => "BondedAnimalKillTracker";

        public BondedAnimalOnFireKillTracker()
        {
        }

        public BondedAnimalOnFireKillTracker(BondedAnimalOnFireKillTracker reference) : base(reference)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override bool Trigger(Pawn pawn)
        {
            base.Trigger(pawn);
            bool kind = kindDef is null || pawn.kindDef == kindDef;
            bool race = raceDef is null || pawn.def == raceDef;
            bool faction = factionDefs.NullOrEmpty() || factionDefs.Contains(pawn.Faction.def);
            bool hitCount = count <= 1 ? true : ++triggeredCount >= count;
            bool bonded = pawn.relations.DirectRelations.Any(d => d.def == PawnRelationDefOf.Bond);
            return kind && race && faction && hitCount && pawn.IsBurning() && bonded;
        }
    }
}
