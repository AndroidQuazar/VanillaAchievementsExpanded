using System;
using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
    public class MultiKillTracker : KillTracker
    {
        public override string Key => "MultiKillTracker";
        protected override string[] DebugText
        {
            get
            {
                string[] text = new string[0];
                foreach (var kind in kindDefDict)
                {
                    triggeredKindDefCount.TryGetValue(kind.Key, out int current);
                    var entry = $"Kind: {kind.Key?.defName ?? "None"} Count: {kind.Value} Current: {current}";
                    text.AddItem(entry);
                }
                foreach (var race in raceDefDict)
                {
                    triggeredRaceDefCount.TryGetValue(race.Key, out int current);
                    var entry = $"Race: {race.Key?.defName ?? "None"} Count: {race.Value} Current: {current}";
                    text.AddItem(entry);
                }
                return base.DebugText.AddRangeToArray(text);
            }
        }

        public MultiKillTracker()
        {
        }

        public MultiKillTracker(MultiKillTracker reference) : base(reference)
        {
            kindDefDict = reference.kindDefDict;
            raceDefDict = reference.raceDefDict;
            triggeredKindDefCount = new Dictionary<PawnKindDef, int>();
            triggeredRaceDefCount = new Dictionary<ThingDef, int>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref kindDefDict, "kindDefDict", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref raceDefDict, "raceDefDict", LookMode.Def, LookMode.Value);

            Scribe_Collections.Look(ref triggeredKindDefCount, "triggeredKindDefCount", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref triggeredRaceDefCount, "triggeredRaceDefCount", LookMode.Def, LookMode.Value);
        }

        public override bool Trigger(Pawn param)
        {
            base.Trigger(param);
            bool kindDef = kindDefDict.EnumerableNullOrEmpty() || kindDefDict.ContainsKey(param.kindDef);
            bool raceDef = raceDefDict.EnumerableNullOrEmpty() || raceDefDict.ContainsKey(param.def);
            if (kindDef && raceDef)
            {
                bool kindDefFound = triggeredKindDefCount.ContainsKey(param.kindDef);
                bool raceDefFound = triggeredRaceDefCount.ContainsKey(param.def);
                if (kindDefFound)
                {
                    triggeredKindDefCount[param.kindDef]++;
                }
                else
                {
                    triggeredKindDefCount.Add(param.kindDef, 1);
                }
                if (raceDefFound)
                {
                    triggeredRaceDefCount[param.def]++;
                }
                else
                {
                    triggeredRaceDefCount.Add(param.def, 1);
                }
                bool universalCount = count > 0;
                bool kindDefCount = (kindDefFound && (triggeredKindDefCount[param.kindDef] >= kindDefDict[param.kindDef] || (universalCount && triggeredKindDefCount[param.kindDef] >= count)));
                bool raceDefCount = (raceDefFound && (triggeredRaceDefCount[param.def] >= raceDefDict[param.def] || (universalCount && triggeredRaceDefCount[param.def] >= count)));
                return kindDefCount && raceDefCount && (factionDef is null || param.Faction.def == factionDef);
            }
            return false;
        }

        public Dictionary<PawnKindDef, int> kindDefDict;
        public Dictionary<ThingDef, int> raceDefDict;

        private Dictionary<PawnKindDef, int> triggeredKindDefCount;
        private Dictionary<ThingDef, int> triggeredRaceDefCount;
    }
}
