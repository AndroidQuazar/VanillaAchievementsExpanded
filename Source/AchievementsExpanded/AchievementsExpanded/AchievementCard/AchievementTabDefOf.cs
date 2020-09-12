using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
    [DefOf]
    public static class AchievementTabDefOf
    {
        static AchievementTabDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(AchievementTabDefOf));
        }

        public static AchievementTabDef Main;
    }
}
