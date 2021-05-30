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
		public ThingDef def;
		public int count = 1;
		public int worth;
		public bool singleTransaction = false;

		protected int triggeredCount;
		protected float triggeredWorth;

		public override string Key => "TraderTracker";

		public override MethodInfo MethodHook => AccessTools.Method(typeof(TradeDeal), nameof(TradeDeal.TryExecute));
		public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.TradeDealComplete));
		protected override string[] DebugText => new string[] { $"Def: {def?.defName ?? "[NoDef]"}", $"Count: {count}", $"Worth: {worth}", $"Single Transaction: {singleTransaction}",
																$"Current: [Count = {triggeredCount}] [Worth = {triggeredWorth}" };

		public TraderTracker()
		{
		}

		public TraderTracker(TraderTracker reference) : base(reference)
		{
			def = reference.def;
			count = reference.count;
			worth = reference.worth;
			singleTransaction = reference.singleTransaction;

			triggeredCount = 0;
			triggeredWorth = 0;
		}

		public override (float, string) PercentComplete
		{
			get
			{
				if (!singleTransaction)
				{
					if (worth > 0 && count <= 1)
					{
						return (triggeredWorth / worth, $"{triggeredWorth} / {worth}");
					}
					else if (worth <= 0)
					{
						return ((float)triggeredCount / count, $"{triggeredCount} / {count}");
					}
				}
				return base.PercentComplete;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref count, "count", 1);
			Scribe_Values.Look(ref worth, "worth");
			Scribe_Values.Look(ref singleTransaction, "singleTransaction");
			Scribe_Values.Look(ref triggeredCount, "triggeredCount", 0);
			Scribe_Values.Look(ref triggeredWorth, "triggeredWorth", 0);
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
			{
				triggeredCount = itemCount;
				triggeredWorth = tradeValue;
			}
			else
			{
				triggeredCount += itemCount;
				triggeredWorth += tradeValue;
			}
			return triggeredCount >= count && triggeredWorth >= worth;
		}
	}
}
