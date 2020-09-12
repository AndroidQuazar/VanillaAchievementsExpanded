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
        public override string Key => "PropertyTracker";

        public override MethodInfo MethodHook => AccessTools.Method(typeof(Prefs), "set_DevMode");
        public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.DevModeToggled));

        public DevModeTracker()
        {
        }

        public DevModeTracker(DevModeTracker reference) : base(reference)
        {
            value = reference.value;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref value, "value");
        }

        public override bool Trigger(bool value) => this.value == value;

        public bool value;
    }
}
