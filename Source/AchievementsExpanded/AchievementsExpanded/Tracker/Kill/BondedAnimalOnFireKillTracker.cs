using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AchievementsExpanded
{
	public class BondedAnimalOnFireKillTracker : KillTracker
	{
		public BondedAnimalOnFireKillTracker()
		{
		}

		public BondedAnimalOnFireKillTracker(BondedAnimalOnFireKillTracker reference) : base(reference)
		{
		}

		public override bool Trigger(Pawn pawn, DamageInfo? dinfo)
		{
			bool bonded = pawn.relations?.DirectRelations?.Any(d => d.def == PawnRelationDefOf.Bond) ?? false;
			return pawn.IsBurning() && bonded && base.Trigger(pawn, dinfo);
		}
	}
}
