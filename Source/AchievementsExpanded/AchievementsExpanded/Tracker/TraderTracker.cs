using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
    public class TraderTracker : Tracker<List<Tradeable>>
    {
        public override string Key => "TraderTracker";

        public override MethodInfo MethodHook => AccessTools.Method(typeof(TradeDeal), nameof(TradeDeal.TryExecute));
        public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.TradeDealComplete));
        protected override string[] DebugText => new string[] { };

        public TraderTracker()
        {
        }

        public TraderTracker(TraderTracker reference) : base(reference)
        {
            def = reference.def;
            count = reference.count;
            worth = reference.worth;
            triggeredCount = 0;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref count, "count", 1);
            Scribe_Values.Look(ref worth, "worth");
            Scribe_Values.Look(ref singleTransaction, "singleTransaction");
            Scribe_Values.Look(ref triggeredCount, "triggeredCount", 0);
        }

        public override bool Trigger(List<Tradeable> tradeables)
        {
            base.Trigger(tradeables);
            float tradeValue = 0f;
            int itemCount = 0;
            foreach (Tradeable item in tradeables)
            {
                if ( (def is null || item.ThingDef == def) )
                {
                    itemCount += item.CountToTransfer;
                    tradeValue += (int)item.GetPriceFor(item.ActionToDo);
                }
            }
            if (singleTransaction)
                triggeredCount = itemCount;
            return triggeredCount >= count && tradeValue >= worth;
        }

        public ThingDef def;
        public int count = 1;
        public int worth;
        public bool singleTransaction = false;

        private int triggeredCount;
    }
}
