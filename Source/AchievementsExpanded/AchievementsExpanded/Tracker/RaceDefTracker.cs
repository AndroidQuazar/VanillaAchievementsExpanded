using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using HarmonyLib;

namespace AchievementsExpanded
{
    public class RaceDefTracker : Tracker<PawnKindDef>
    {
        public override string Key => "RaceDefTracker";

        public override MethodInfo MethodHook => AccessTools.Method(typeof(StoryWatcher_PopAdaptation), nameof(StoryWatcher_PopAdaptation.Notify_PawnEvent));
        public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.PawnJoinedFaction));

        public RaceDefTracker()
        {
        }

        public RaceDefTracker(RaceDefTracker reference) : base(reference)
        {
            pawnKinds = reference.pawnKinds;
            total = reference.total;
            requireAll = reference.requireAll;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref pawnKinds, "pawnKinds", LookMode.Def, LookMode.Value);
            Scribe_Values.Look(ref total, "total");
            Scribe_Values.Look(ref requireAll, "requireAll", true);
        }

        public override bool Trigger(PawnKindDef param)
        {
            bool trigger = true;
            if (total)
            {

            }
            else
            {
                var factionPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction;
                if (factionPawns is null)
                    return false;
                foreach (KeyValuePair<PawnKindDef, int> set in pawnKinds)
                {
                    var count = 0;
                    if (set.Key == param)
                        count += 1;
                    if (requireAll)
                    {
                        if (factionPawns.Where(f => f.kindDef.defName == set.Key.defName).Count() + count < set.Value)
                        {
                            trigger = false;
                        }
                    }
                    else
                    {
                        trigger = false;
                        if (factionPawns.Where(f => f.kindDef.defName == set.Key.defName).Count() + count >= set.Value)
                        {
                            return true;
                        }
                    }
                }
            }
            return trigger;
        }

        Dictionary<PawnKindDef, int> pawnKinds = new Dictionary<PawnKindDef, int>();
        public bool total;
        public bool requireAll = true;
    }
}
