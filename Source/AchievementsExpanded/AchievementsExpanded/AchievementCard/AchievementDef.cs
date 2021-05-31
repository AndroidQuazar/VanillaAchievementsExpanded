using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace AchievementsExpanded
{
	public class AchievementDef : Def
	{
		public string texPath;
		public string bgtexPath;

		public new string description;
		public int order = 9999;

		public int points;
		public TrackerBase tracker;

		public AchievementTabDef tab;
		public Type achievementClass;
	}
}
