using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace AchievementsExpanded
{
    [StaticConstructorOnStartup]
    public static class AchievementTex
    {
        static AchievementTex()
        {
            if (ModsConfig.IsActive(AchievementHarmony.modIdentifier))
            {
                PointsIcon = ContentFinder<Texture2D>.Get("Icons/IconStorytellerPoints");
                PointsIconDark = ContentFinder<Texture2D>.Get("Icons/IconStorytellerPointsDark");
                CardBG = ContentFinder<Texture2D>.Get("Achievements/AchievementBackground");
            }
        }

        public static Texture2D PointsIcon;

        public static Texture2D PointsIconDark;

        public static Texture2D CardBG;
    }
}
