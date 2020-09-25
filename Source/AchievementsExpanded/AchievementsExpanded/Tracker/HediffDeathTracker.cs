using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AchievementsExpanded
{
    public class HediffDeathTracker : Tracker<Hediff>
    {
        public override string Key => "ColonistDeathTracker";

        public override MethodInfo MethodHook => AccessTools.Method(typeof(Hediff), nameof(Hediff.Notify_PawnDied));
        public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.HediffDeathEvent));
        protected override string[] DebugText => new string[] { $"Def: {def.defName}"};

        public HediffDeathTracker()
        {
        }

        public HediffDeathTracker(HediffDeathTracker reference) : base(reference)
        {
            def = reference.def;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref def, "def");
        }

        public override bool Trigger(Hediff hediff)
        {
            return hediff.def == def;
        }

        public HediffDef def;
    }
}
