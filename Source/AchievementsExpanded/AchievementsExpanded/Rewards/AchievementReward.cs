using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AchievementsExpanded
{
	public abstract class AchievementReward : Def
	{
		public int cost;
		public IncidentDef incident;
		public AchievementTabDef tab;

		/// <summary>
		/// Reason for being disabled (if empty, user can purchase)
		/// </summary>
		public virtual string Disabled
		{
			get
			{
				string reason = string.Empty;
				if (!DebugSettings.godMode && Current.Game.GetComponent<AchievementPointManager>().availablePoints < cost)
				{
					reason += "NotEnoughPoints".Translate();
				}
				return reason;
			}
		}

		/// <summary>
		/// Attempt to purchase reward
		/// </summary>
		public virtual bool PurchaseReward() => Current.Game.GetComponent<AchievementPointManager>().TryPurchasePoints(cost);

		/// <summary>
		/// Refund reward purchased
		/// </summary>
		/// <returns></returns>
		public virtual void RefundPoints() => Current.Game.GetComponent<AchievementPointManager>().RefundPoints(cost);

		/// <summary>
		/// Try Execute reward post-purchase
		/// </summary>
		public abstract bool TryExecuteEvent();
	}
}
