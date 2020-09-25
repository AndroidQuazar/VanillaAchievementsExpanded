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
                return new string[] { };
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

        public override bool Trigger(Pawn pawn)
        {
            base.Trigger(pawn);
            bool kindDef = kindDefDict.EnumerableNullOrEmpty() || kindDefDict.ContainsKey(pawn.kindDef);
            bool raceDef = raceDefDict.EnumerableNullOrEmpty() || raceDefDict.ContainsKey(pawn.def);
            if (kindDef && raceDef)
            {
                bool kindDefFound = triggeredKindDefCount.ContainsKey(pawn.kindDef);
                bool raceDefFound = triggeredRaceDefCount.ContainsKey(pawn.def);
                if (kindDefFound)
                {
                    triggeredKindDefCount[pawn.kindDef]++;
                }
                else
                {
                    triggeredKindDefCount.Add(pawn.kindDef, 1);
                }
                if (raceDefFound)
                {
                    triggeredRaceDefCount[pawn.def]++;
                }
                else
                {
                    triggeredRaceDefCount.Add(pawn.def, 1);
                }
                bool universalCount = count > 0;
                bool kindDefCount = (kindDefFound && (triggeredKindDefCount[pawn.kindDef] >= kindDefDict[pawn.kindDef] || (universalCount && triggeredKindDefCount[pawn.kindDef] >= count)));
                bool raceDefCount = (raceDefFound && (triggeredRaceDefCount[pawn.def] >= raceDefDict[pawn.def] || (universalCount && triggeredRaceDefCount[pawn.def] >= count)));
                return kindDefCount && raceDefCount && factionDefs.NullOrEmpty() || factionDefs.Contains(pawn.Faction.def);
            }
            return false;
        }

        public Dictionary<PawnKindDef, int> kindDefDict;
        public Dictionary<ThingDef, int> raceDefDict;

        private Dictionary<PawnKindDef, int> triggeredKindDefCount;
        private Dictionary<ThingDef, int> triggeredRaceDefCount;
    }
}
