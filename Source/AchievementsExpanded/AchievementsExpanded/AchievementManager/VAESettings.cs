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


        public override void ExposeData()
        {
            base.ExposeData();
        }
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
        }
    }
}
