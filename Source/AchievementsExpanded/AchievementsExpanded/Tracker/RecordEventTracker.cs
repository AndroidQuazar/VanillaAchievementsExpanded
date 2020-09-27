using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AchievementsExpanded
{
    public class RecordEventTracker : Tracker2<RecordDef, Pawn>
    {
        public override string Key => "RecordEventTracker";
        public override MethodInfo MethodHook => AccessTools.Method(typeof(Pawn_RecordsTracker), nameof(Pawn_RecordsTracker.Increment)); //Patch on AddTo as well
        public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.RecordEvent));
        protected override string[] DebugText => new string[] { $"Def: {def.defName}", $"Count: {count}" };

        public RecordEventTracker()
        {
        }

        public RecordEventTracker(RecordEventTracker reference) : base(reference)
        {
            def = reference.def;
            count = reference.count;
            total = reference.total;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref count, "num");
            Scribe_Values.Look(ref total, "total");
        }

        public override bool Trigger(RecordDef record, Pawn pawn)
        {
            base.Trigger(record, pawn);
            if (total)
            {
                float value = 0;
                foreach (Map map in Find.Maps)
                {
                    foreach (Pawn pawn2 in map.mapPawns.FreeColonists)
                    {
                        value += pawn2.records.GetValue(record);
                        if (value >= count)
                            return true;
                    }
                    if (value >= count)
                            return true;
                }
                return false;
            }
            return pawn.records.GetValue(def) >= count;
        }

        public RecordDef def;
        public float count;
        public bool total;
    }
}
