using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace AchievementsExpanded
{
	/// <summary>
	/// Autotriggers when MethodHook is called
	/// </summary>
	public class DevModeTracker : Tracker<bool>
	{
		public bool value;

		public override string Key => "DevModeTracker";

		public override MethodInfo MethodHook => AccessTools.Method(typeof(Prefs), "set_DevMode");
		public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.DevModeToggled));
		protected override string[] DebugText => new string[] { $"Value: {value}" };

		public DevModeTracker()
		{
		}

		public DevModeTracker(DevModeTracker reference) : base(reference)
		{
			value = reference.value;
		}

		public override bool UnlockOnStartup => Trigger(Prefs.DevMode);

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref value, "value");
		}

		public override bool Trigger(bool value)
		{
			base.Trigger(value);
			return this.value == value;
		}
	}
}
