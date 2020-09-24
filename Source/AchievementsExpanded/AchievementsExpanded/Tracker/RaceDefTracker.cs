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
        protected override string[] DebugText
        {
            get
            {
                string[] text = new string[0];
                foreach (var kind in kindDefDict)
                {
                    string entry = $"Kind: {kind.Key?.defName ?? "None"} Count: {kind.Value}";
                    text.AddItem(entry);
                }
                text.AddItem($"Total over time: {total}");
                text.AddItem($"Require all in list: {requireAll}");
                return text;
            }
        }

        public RaceDefTracker()
        {
        }

        public RaceDefTracker(RaceDefTracker reference) : base(reference)
        {
            kindDefDict = reference.kindDefDict;
            if (kindDefDict.EnumerableNullOrEmpty())
                Log.Error($"KindDef list for RaceDefTracker cannot be Null or Empty");
            total = reference.total;
            requireAll = reference.requireAll;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref kindDefDict, "kindDefDict", LookMode.Def, LookMode.Value);
            Scribe_Values.Look(ref total, "total");
            Scribe_Values.Look(ref requireAll, "requireAll", true);
        }

        public override bool Trigger(PawnKindDef param)
        {
            base.Trigger(param);
            bool trigger = true;
            if (total)
            {

            }
            else
            {
                var factionPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction;
                if (factionPawns is null)
                    return false;
                foreach (KeyValuePair<PawnKindDef, int> set in kindDefDict)
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

        Dictionary<PawnKindDef, int> kindDefDict = new Dictionary<PawnKindDef, int>();
        public bool total;
        public bool requireAll = true;
    }
}
