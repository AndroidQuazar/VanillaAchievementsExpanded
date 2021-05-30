using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace AchievementsExpanded
{
	public class VAESettings : ModSettings
	{
		public bool writeAllSettings;
	}

	public class VAEMod : Mod
	{
		public static VAESettings settings;

		public VAEMod(ModContentPack content) : base(content)
		{
			settings = GetSettings<VAESettings>();
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			base.DoSettingsWindowContents(inRect);
			var listing = new Listing_Standard();
			Rect buttonRect = new Rect(inRect)
			{
				width = inRect.width / 5
			};
			listing.Begin(buttonRect);
			//listing.ConfirmationBoxCheckboxLabeled("DebugWriter".Translate(), ref settings.writeAllSettings);
			if (listing.ButtonText("GenerateLogInfo".Translate(), "GenerateLogInfoTooltip".Translate()))
			{
				DebugWriter.PushToFile();
			}
			listing.End();
		}

		public override string SettingsCategory()
		{
			return UtilityMethods.BaseModActive ? "VAE".Translate().ToString() : string.Empty;
		}
	}
}
