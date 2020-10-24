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
        public bool devModeUnlocked;

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
            tracker.cardAssigned = this.def.defName;
        }

        public string GetUniqueLoadID()
        {
            return $"Achievement_{uniqueHash}";
        }

        public virtual void UnlockCard(bool debugTools = false)
        {
            if(!unlocked)
            {
                unlocked = true;

                var vector = Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile);
                dateUnlocked = (Prefs.DevMode || debugTools) ? "UnlockedDevMode".Translate().ToString() : GenDate.DateReadoutStringAt(Find.TickManager.TicksAbs, vector);

                Current.Game.GetComponent<AchievementPointManager>().AddPoints(def.points);
                DebugWriter.Log($"Unlocking: {GetUniqueLoadID()} Card: {def.label}");
                if (debugTools)
                {
                    DebugWriter.Log($"[Unlocked with DebugTools]");
                }
                Current.Game.GetComponent<AchievementPointManager>().DisplayCardWindow(this);
            }
        }

        public virtual void DrawCard(Rect rect)
        {
            Rect iconRect = new Rect(rect.x, rect.y, rect.width, rect.width).ContractedBy(MainTabWindow_Achievements.SpaceBetweenCards);

            Widgets.DrawMenuSection(rect);
            GUI.DrawTexture(iconRect, AchievementBGIcon);

            Rect innerIconRect = iconRect.ContractedBy(3);
            var color = GUI.color;
            if(!unlocked && !BadTex)
                GUI.color = Color.black;
            GUI.DrawTexture(innerIconRect, AchievementIcon);
            GUI.color = color;

            var anchor = Text.Anchor;
            Text.Anchor = TextAnchor.UpperCenter;

            Rect labelRect = new Rect(iconRect.x, iconRect.y + iconRect.height, iconRect.width, rect.height - MainTabWindow_Achievements.SpaceBetweenCards - iconRect.height);
            Widgets.DrawBox(labelRect, 1);
            Widgets.Label(labelRect, def.label);

            var font = Text.Font;
            var textColor = GUI.color;
            Text.Font = GameFont.Tiny;
            GUI.color = MainTabWindow_Achievements.LightGray;

            var size = Text.CalcSize(def.description);
            Rect pointRect = new Rect(iconRect.x, labelRect.y + MainTabWindow_Achievements.SpaceBetweenCards, iconRect.width, labelRect.height);
            Rect pointIconRect = new Rect(iconRect.x, labelRect.y + MainTabWindow_Achievements.SpaceBetweenCards * 2, size.y, size.y);
            Widgets.Label(pointRect, def.points.ToStringSafe());
            GUI.DrawTexture(pointIconRect, AchievementTex.PointsIcon);

            var descTextFull = $"{def.points} - {def.description}";
            Rect descRect = new Rect(iconRect.x, pointIconRect.y + MainTabWindow_Achievements.SpaceBetweenCards * 5f, iconRect.width, labelRect.height);
            Widgets.Label(descRect, def.description);

            GUI.color = Color.gray;
            var timeSize = Text.CalcSize(dateUnlocked);
            var timeWidth = Mathf.Clamp(timeSize.x, 0f, iconRect.width);
            Rect unlockTimeRect = new Rect(rect.x + (rect.width / 2) - (timeWidth / 2), iconRect.y + rect.height - (timeSize.y * 1.5f), rect.width, timeSize.y);
            Widgets.Label(unlockTimeRect, dateUnlocked);

            Text.Font = font;
            GUI.color = textColor;
            Text.Anchor = anchor;
        }

        internal void LockCard()
        {
            unlocked = false;
            devModeUnlocked = false;
            dateUnlocked = "AchievementLocked".Translate();
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Defs.Look(ref tab, "tab");

            Scribe_Deep.Look(ref tracker, "tracker");

            Scribe_Values.Look(ref unlocked, "unlocked", false);
            Scribe_Values.Look(ref devModeUnlocked, "devModeUnlocked", false);
            Scribe_Values.Look(ref dateUnlocked, "dateUnlocked", "Locked");

            Scribe_Values.Look(ref uniqueHash, "uniqueHash");
        }
    }
}
