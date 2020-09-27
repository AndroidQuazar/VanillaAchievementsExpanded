using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AchievementsExpanded
{
    public class Reward_EnemyRaid : Reward_MapIncident
    {
        public override bool TryExecuteEvent()
        {
            IncidentParms parms = new IncidentParms()
            {
                target = Find.CurrentMap,
                points = StorytellerUtility.DefaultThreatPointsNow(Find.CurrentMap) * 2.5f,
                faction = Find.World.factionManager.RandomEnemyFaction(true),
            };
            if (!incident.Worker.TryExecute(parms))
            {
                Messages.Message("FailedRewardEvent".Translate(defName), MessageTypeDefOf.RejectInput);
                return false;
            }
            return true;
        }
    }
}
