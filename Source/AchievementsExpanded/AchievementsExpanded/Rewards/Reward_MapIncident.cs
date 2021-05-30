using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AchievementsExpanded
{
	public class Reward_MapIncident : AchievementReward
	{
		public override string Disabled
		{
			get
			{
				string reason = base.Disabled;
				if (!Find.CurrentMap?.IsPlayerHome ?? false)
				{
					reason += "\n" + "NoValidMap".Translate();
				}
				if (Parms is null || !incident.Worker.CanFireNow(Parms))
				{
					reason += "\n" + "IncidentNotAvailable".Translate();
				}
				return reason;
			}
		}

		protected IncidentParms Parms => Find.CurrentMap != null ? StorytellerUtility.DefaultParmsNow(incident.category, Find.CurrentMap) : null;

		public override bool TryExecuteEvent()
		{
			
			if (!incident.Worker.TryExecute(Parms))
			{
				Messages.Message("FailedRewardEvent".Translate(defName), MessageTypeDefOf.RejectInput);
				return false;
			}
			return true;
		}
	}
}
