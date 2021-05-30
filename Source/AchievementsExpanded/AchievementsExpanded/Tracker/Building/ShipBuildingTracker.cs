using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
	public class ShipBuildingTracker : BuildingTracker
	{
		protected override string[] DebugText =>  base.DebugText;

		public ShipBuildingTracker()
		{
		}

		public ShipBuildingTracker(ShipBuildingTracker reference) : base(reference)
		{
		}

		public override bool Trigger(Building building)
		{
			base.Trigger(building);
			if (building.Faction != Faction.OfPlayer || building.Map is null)
			{
				return false;
			}
			List<Building> shipParts = ShipUtility.ShipBuildingsAttachedTo(building).ToList();
			bool missingParts = false;
			using (Dictionary<ThingDef, int>.Enumerator enumerator = ShipUtility.RequiredParts().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<ThingDef, int> partDef = enumerator.Current;
					int num = shipParts.Count((Building pa) => pa.def == partDef.Key);
					if (num < partDef.Value)
					{
						//DebugWriter.Log(string.Format("{0}: {1}x {2} ({3} {4})", new object[]
						//{
						//	"ShipReportMissingPart".Translate(),
						//	partDef.Value - num,
						//	partDef.Key.label,
						//	"ShipReportMissingPartRequires".Translate(),
						//	partDef.Value
						//}));
						missingParts = true;
					}
				}
			}
			return !missingParts;
		}
	}
}
