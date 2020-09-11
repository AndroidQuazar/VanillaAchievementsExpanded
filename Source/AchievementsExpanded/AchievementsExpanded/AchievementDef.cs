using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse
{
    public class AchievementDef : Def
    {
        public string texPath;
        public int points;
        public AchievementsExpanded.AchievementTabDef tab;
        //Temp
        public string tracker;
    }
}
