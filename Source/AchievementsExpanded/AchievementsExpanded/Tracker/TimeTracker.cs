using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
    public class TimeTracker : TrackerBase
    {
        public override string Key => "TimeTracker";

        public override MethodInfo MethodHook => AccessTools.Method(typeof(TickManager), nameof(TickManager.TickManagerUpdate));
        public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.TimeTickPassed));
        protected override string[] DebugText => new string[] { $"Ticks: {ticksPassed}", $"Require unpaused to Tick: {gameTime}" };
        public TimeTracker()
        {
        }

        public TimeTracker(TimeTracker reference) : base(reference)
        {
            ticksPassed = reference.ticksPassed;
            gameTime = reference.gameTime;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksPassed, "ticksPassed");
            Scribe_Values.Look(ref gameTime, "gameTime");
        }
        public override bool Trigger()
        {
            base.Trigger();
            int ticks = gameTime ? Find.TickManager.TicksGame : Find.TickManager.TicksAbs;
            return ticks >= ticksPassed;
        }

        public int ticksPassed;
        public bool gameTime = true;
    }
}
