using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace AchievementsExpanded
{
    public class AchievementCard : IExposable, ILoadReferenceable
    {
        public AchievementDef def;
        public AchievementTabDef tab;
        public TrackerBase tracker;

        public bool unlocked;

        public int uniqueHash = -1;
        public string dateUnlocked = "Locked";

        public bool BadTex { get; set; }
        private Texture2D achievementIcon;
        public Texture2D AchievementIcon
        {
            get
            {
                if (achievementIcon is null)
                {
                    achievementIcon = ContentFinder<Texture2D>.Get(def.texPath, false);
                    if (achievementIcon is null)
                    { 
                        achievementIcon = BaseContent.BadTex;
                        BadTex = true;
                    }
                }
                return achievementIcon;
            }
        }

        private Texture2D achievementBGIcon;
        public Texture2D AchievementBGIcon
        {
            get
            {
                if(achievementBGIcon is null)
                {
                    if(string.IsNullOrEmpty(def.bgtexPath))
                    {
                        achievementBGIcon = AchievementTex.CardBG;
                    }
                    else
                    {
                        achievementBGIcon = ContentFinder<Texture2D>.Get(def.bgtexPath);
                    }
                }
                return achievementBGIcon;
            }
        }

        public AchievementCard()
        {
        }

        public AchievementCard(AchievementDef def, bool preUnlocked = false)
        {
            this.def = def;
            tab = def.tab;
            if(tab is null)
            {
                tab = AchievementTabDefOf.Main;
            }
            uniqueHash = def.defName.GetHashCode();
            unlocked = preUnlocked;
            tracker = (TrackerBase)Activator.CreateInstance(def.tracker.GetType(), new object[] { def.tracker });
        }

        public string GetUniqueLoadID()
        {
            return $"Achievement_{uniqueHash}";
        }

        public void UnlockCard()
        {
            if(!unlocked)
            {
                unlocked = true;
                var vector = Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile);
                dateUnlocked = GenDate.DateReadoutStringAt(Find.TickManager.TicksAbs, vector);

                Current.Game.GetComponent<AchievementPointManager>().availablePoints += def.points;

                Find.WindowStack.Add(new AchievementNotification(this));
            }
        }

        internal void LockCard()
        {
            unlocked = false;
            dateUnlocked = "Locked";
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Defs.Look(ref tab, "tab");

            Scribe_Deep.Look(ref tracker, "tracker");

            Scribe_Values.Look(ref unlocked, "unlocked", false);
            Scribe_Values.Look(ref dateUnlocked, "dateUnlocked", "Locked");

            Scribe_Values.Look(ref uniqueHash, "uniqueHash");
        }
    }
}
