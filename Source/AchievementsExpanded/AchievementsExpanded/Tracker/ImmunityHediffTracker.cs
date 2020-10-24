using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AchievementsExpanded
{
    public class ImmunityHediffTracker : Tracker2<Hediff, float>
    {
        public override string Key => "ImmunityHediffTracker";
        public override MethodInfo MethodHook => AccessTools.Method(typeof(ImmunityRecord), nameof(ImmunityRecord.ImmunityTick));
        public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.ImmunityTicking));
        protected override string[] DebugText => new string[] { $"ImmunityLevel: {percentImmune}" };

        public ImmunityHediffTracker()
        {
        }

        public ImmunityHediffTracker(ImmunityHediffTracker reference) : base(reference)
        {
            percentImmune = reference.percentImmune;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref percentImmune, "percentImmune");
        }

        public override bool Trigger(Hediff hediff, float immunity)
        {
            base.Trigger(hediff);
            if (def is null || hediff?.def == def)
            {
                if (hediff.Severity >= percentImmune && immunity >= 99)
                {
                    return true;
                }
            }
            return false;
        }

        public float percentImmune;
        public HediffDef def;
    }
}
