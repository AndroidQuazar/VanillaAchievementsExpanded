using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AchievementsExpanded
{
	public class IncidentSeasonalTracker : IncidentTracker
	{
		public List<Season> allowedSeasons;

		public IncidentSeasonalTracker()
		{
		}

		public IncidentSeasonalTracker(IncidentSeasonalTracker reference) : base(reference)
		{
			allowedSeasons = reference.allowedSeasons;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref allowedSeasons, "allowedSeasons", LookMode.Value);
		}

		public override bool Trigger(IncidentDef param, Map map)
		{
			if(map is null) return false;
			if (!allowedSeasons?.Contains(GenLocalDate.Season(map.Tile)) ?? true)
			{
				return false;
			}
			return base.Trigger(param, map);
		}
	}
}
