using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using HarmonyLib;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
    public class ItemTracker : TrackerBase
    {
        public override string Key => "ItemTracker";

        public override Func<bool> AttachToLongTick => () => { return Trigger(); };
        protected override string[] DebugText => new string[] { $"Def: {def?.defName ?? "None"}", $"Count: {count}" };

        public ItemTracker()
        {
        }

        public ItemTracker(ItemTracker reference) : base(reference)
        {
            def = reference.def;
            count = reference.count;
            if (count <= 0)
                count = 1;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref count, "count", 1);
        }

        public override bool Trigger()
        {
            base.Trigger();
            return UtilityMethods.PlayerHas(def, out int total, count);
        }

        public ThingDef def;
        public int count;
    }
}
