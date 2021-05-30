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
		public int ticksPassed;
		public bool gameTime = true;

		public override string Key => "TimeTracker";

		public override Func<bool> AttachToLongTick => () => { return Trigger(); };
		protected override string[] DebugText => new string[] { $"Ticks: {ticksPassed}", $"Require unpaused to Tick: {gameTime}", $"Current Abs: {(int)Find.GameInfo.RealPlayTimeInteracting} Current Game: {Find.TickManager.TicksGame}"};
		public TimeTracker()
		{
		}

		public TimeTracker(TimeTracker reference) : base(reference)
		{
			ticksPassed = reference.ticksPassed;
			gameTime = reference.gameTime;
		}

		public override bool UnlockOnStartup => Trigger();

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref ticksPassed, "ticksPassed");
			Scribe_Values.Look(ref gameTime, "gameTime");
		}
		public override bool Trigger()
		{
			base.Trigger();
			int ticks = gameTime ? Find.TickManager.TicksGame : (int)Find.GameInfo.RealPlayTimeInteracting;
			return ticks >= ticksPassed;
		}
	}
}
