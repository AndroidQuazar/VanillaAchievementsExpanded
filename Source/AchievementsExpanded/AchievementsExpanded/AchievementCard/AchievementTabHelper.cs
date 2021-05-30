using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AchievementsExpanded
{
	[StaticConstructorOnStartup]
	public static class AchievementTabHelper
	{
		public static AchievementTabDef MainTab;

		static AchievementTabHelper()
		{
			MainTab = DefDatabase<AchievementTabDef>.GetNamedSilentFail("Main");
		}
	}
}
