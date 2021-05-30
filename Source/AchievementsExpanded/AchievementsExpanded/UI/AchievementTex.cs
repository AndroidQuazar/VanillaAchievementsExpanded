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
			if (UtilityMethods.BaseModActive)
			{
				PointsIcon = ContentFinder<Texture2D>.Get("Icons/IconStorytellerPoints");
				PointsIconDark = ContentFinder<Texture2D>.Get("Icons/IconStorytellerPointsDark");
				CardBG = ContentFinder<Texture2D>.Get("Achievements/AchievementBackground");
			}
		}

		public static Texture2D PointsIcon;

		public static Texture2D PointsIconDark;

		public static Texture2D CardBG;

		public static readonly Texture2D DefaultBarBgTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.1f, 0.1f));

		public static readonly Texture2D BarFullTexHor = SolidColorMaterials.NewSolidColorTexture(new Color(0.35f, 0.35f, 0.2f));
	}
}
