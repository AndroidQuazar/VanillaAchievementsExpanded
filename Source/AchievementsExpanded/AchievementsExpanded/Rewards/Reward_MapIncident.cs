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
                return reason;
            }
        }

        public override bool TryExecuteEvent()
        {
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(incident.category, Find.CurrentMap);
            if (!incident.Worker.TryExecute(parms))
            {
                Messages.Message("FailedRewardEvent".Translate(defName), MessageTypeDefOf.RejectInput);
                return false;
            }
            return true;
        }
    }
}
