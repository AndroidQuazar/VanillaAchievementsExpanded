using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
    public class ItemCraftTracker : Tracker<Thing>
    {
        public override string Key => "ItemCraftTracker";

        public override MethodInfo MethodHook => AccessTools.Method(typeof(QuestManager), nameof(QuestManager.Notify_ThingsProduced));
        public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.ThingSpawned));
        
        public ItemCraftTracker()
        {
        }

        public ItemCraftTracker(ItemCraftTracker reference) : base(reference)
        {
            def = reference.def;
            madeFrom = reference.madeFrom;
            quality = reference.quality;
            count = reference.count;
            triggeredCount = 0;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref def, "def");
            Scribe_Defs.Look(ref madeFrom, "madeFrom");
            Scribe_Values.Look(ref quality, "quality");
            Scribe_Values.Look(ref count, "count");
            Scribe_Values.Look(ref triggeredCount, "triggeredCount");
        }

        public override bool Trigger(Thing thing)
        {
            if ((def is null || thing.def == def) && (madeFrom is null || madeFrom == thing.Stuff))
            {
                if (quality is null || (thing.TryGetQuality(out var qc) && qc >= quality))
                {
                    triggeredCount++;
                }
            }
            return triggeredCount >= count;
        }

        public ThingDef def;
        public ThingDef madeFrom;
        public QualityCategory? quality;
        public int count;

        private int triggeredCount;
    }
}
