using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AchievementsExpanded
{
    [StaticConstructorOnStartup]
    public static class AchievementTabHelper
    { 
        static AchievementTabHelper()
        {
            MainTab = DefDatabase<AchievementTabDef>.GetNamedSilentFail("Main");
        }

        public static AchievementTabDef MainTab;
    }
}
