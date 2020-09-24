using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
    public class MultiHediffTracker : HediffTracker
    {
        public override string Key => "MultiHediffTracker";

        public MultiHediffTracker()
        {
        }

        public MultiHediffTracker(MultiHediffTracker reference) : base(reference)
        {
            defs = reference.defs;
            triggeredCountDict = new Dictionary<Tuple<Pawn, HediffDef, BodyPartRecord>, int>();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref defs, "defs", LookMode.Def, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                tmpPawns = new List<Pawn>();
                tmpDefs = new List<HediffDef>();
                tmpCounts = new List<int>();
                tmpBodyParts = new List<BodyPartRecord>();
                foreach (var pairing in triggeredCountDict)
                {
                    tmpPawns.Add(pairing.Key.Item1);
                    tmpDefs.Add(pairing.Key.Item2);
                    tmpBodyParts.Add(pairing.Key.Item3);
                    tmpCounts.Add(pairing.Value);
                }
            }
            Scribe_Collections.Look(ref tmpPawns, "tmpPawns", LookMode.Reference);
            Scribe_Collections.Look(ref tmpDefs, "tmpDefs");
            Scribe_Collections.Look(ref tmpCounts, "tmpCounts");
            Scribe_Collections.Look(ref tmpBodyParts, "tmpBodyParts");
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                triggeredCountDict = new Dictionary<Tuple<Pawn, HediffDef, BodyPartRecord>, int>();
                for(int i = 0; i < defs.Count; i++)
                {
                    triggeredCountDict.Add(new Tuple<Pawn, HediffDef, BodyPartRecord>(tmpPawns[i], tmpDefs[i], tmpBodyParts[i]), tmpCounts[i]);
                }
            }
        }

        public override bool Trigger(Hediff hediff)
        {
            base.Trigger(hediff);
            var pair = new Tuple<Pawn, HediffDef, BodyPartRecord>(hediff.pawn, hediff.def, hediff.Part);
            if (defs.ContainsKey(hediff.def))
            {
                if (!triggeredCountDict.ContainsKey(pair))
                {
                    triggeredCountDict.Add(new Tuple<Pawn, HediffDef, BodyPartRecord>(hediff.pawn, hediff.def, hediff.Part), 0);
                }
                triggeredCountDict[pair]++;
                Log.Message($"Registered hediff: {hediff.def} Count: {triggeredCountDict[pair]} BodyPart: {hediff.Part}");
                if (triggeredCountDict[pair] >= defs[hediff.def])
                    return true;
            }
            return false;
        }

        public Dictionary<HediffDef, int> defs = new Dictionary<HediffDef, int>();
        public Dictionary<Tuple<Pawn, HediffDef, BodyPartRecord>, int> triggeredCountDict = new Dictionary<Tuple<Pawn, HediffDef, BodyPartRecord>, int>();

        private List<Pawn> tmpPawns;
        private List<HediffDef> tmpDefs;
        private List<BodyPartRecord> tmpBodyParts;
        private List<int> tmpCounts;
    }
}
