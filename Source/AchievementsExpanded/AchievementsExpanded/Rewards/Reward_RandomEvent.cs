using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace AchievementsExpanded
{
	public class Reward_RandomEvent : AchievementReward
	{
		public override string Disabled
		{
			get
			{
				string reason = base.Disabled;
				if (Find.CurrentMap is null)
				{
					reason += "\n" + "NoValidMap".Translate();
				}
				return reason;
			}
		}

		public override bool TryExecuteEvent()
		{
			List<IncidentDef> incidents = DefDatabase<IncidentDef>.AllDefsListForReading.ToList();
			Rand.PushState();
			incidents.Shuffle();
			Rand.PopState();
			IIncidentTarget target = WorldRendererUtility.WorldRenderedNow ? (Find.WorldSelector.SingleSelectedObject as IIncidentTarget) : null;
			if (target is null)
			{
				target = Find.CurrentMap;
			}
			if (target is null)
			{
				Messages.Message("FailedTargetFinder".Translate(), MessageTypeDefOf.RejectInput);
				return false;
			}    

			foreach (IncidentDef incident in incidents)
			{
				try
				{
					var parms = StorytellerUtility.DefaultParmsNow(incident.category, target);
					if (incident.Worker.CanFireNow(parms) && incident.Worker.TryExecute(parms))
					{
						return true;
					}
				}
				catch
				{
				}
			}
			Messages.Message("FailedRewardEvent".Translate(defName), MessageTypeDefOf.RejectInput);
			return false;
		}
	}
}
