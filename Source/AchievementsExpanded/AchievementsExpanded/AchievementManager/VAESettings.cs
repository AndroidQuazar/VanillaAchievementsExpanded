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
            listing.Begin(inRect);
            listing.ConfirmationBoxCheckboxLabeled("DebugWriter".Translate(), ref settings.writeAllSettings);
            listing.End();
        }

        public override string SettingsCategory()
        {
            return "VAE".Translate();
        }
    }
}
