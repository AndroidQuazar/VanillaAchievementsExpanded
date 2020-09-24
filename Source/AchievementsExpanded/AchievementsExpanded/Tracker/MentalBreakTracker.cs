﻿using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using Verse.AI;
using RimWorld;

namespace AchievementsExpanded
{
    public class MentalBreakTracker : Tracker<MentalBreakDef>
    {
        public override string Key => "MentalBreakTracker";

        public override MethodInfo MethodHook => AccessTools.Method(typeof(MentalBreaker), nameof(MentalBreaker.TryDoRandomMoodCausedMentalBreak));
        public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.MentalBreakTriggered));
        public override PatchType PatchType => PatchType.Transpiler;
        protected override string[] DebugText => new string[] { $"Def: {def?.defName ?? "None"}", $"Count: {count}", $"Consecutive: {consecutive}", $"Current: {triggeredCount}" };
        public MentalBreakTracker()
        {
        }

        public MentalBreakTracker(MentalBreakTracker reference) : base(reference)
        {
            def = reference.def;
            count = reference.count;
            consecutive = reference.consecutive;
            if (count < 1)
                count = 1;
            triggeredCount = 0;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref count, "count");
            Scribe_Values.Look(ref consecutive, "consecutive", 1);
            Scribe_Values.Look(ref triggeredCount, "triggeredCount", 0);
        }

        public override bool Trigger(MentalBreakDef def)
        {
            base.Trigger(def);
            if( (this.def is null || this.def == def) )
            {
                if (consecutive > 1)
                {
                    int total = 0;
                    foreach(Pawn colonist in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists)
                    {
                        if (colonist.InMentalState && (this.def is null || colonist.MentalStateDef == def.mentalState))
                        {
                            total += 1;
                        }
                    }
                    if (logTracker || VAEMod.settings.writeAllSettings)
                        DebugWriter.Log($"Total in Mental State: {total}");
                    if (total < consecutive)
                        return false;
                }
                triggeredCount++;
                return triggeredCount >= count;
            }
            return false;
        }

        public MentalBreakDef def;
        public int count;
        public int consecutive;

        private int triggeredCount;
    }
}
