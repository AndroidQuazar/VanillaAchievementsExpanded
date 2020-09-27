using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace AchievementsExpanded
{
    public class Reward_Random : AchievementReward
    {
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
                    if (incident.Worker.TryExecute(StorytellerUtility.DefaultParmsNow(incident.category, target)))
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
