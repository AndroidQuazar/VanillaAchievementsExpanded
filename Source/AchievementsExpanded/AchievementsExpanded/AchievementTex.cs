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
        public static readonly Texture2D PointsIcon = ContentFinder<Texture2D>.Get("Icons/IconStorytellerPoints");

        public static readonly Texture2D PointsIconDark = ContentFinder<Texture2D>.Get("Icons/IconStorytellerPointsDark");

        public static readonly Texture2D CardBG = ContentFinder<Texture2D>.Get("Achievements/AchievementBackground");
    }
}
