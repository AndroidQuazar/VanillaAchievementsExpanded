using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;
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

		private Texture2D achievementIcon;
		private Texture2D achievementBGIcon;

		public AchievementCard()
		{
		}

		public AchievementCard(AchievementDef def, bool preUnlocked = false)
		{
			this.def = def;
			tab = def.tab;
			if (tab is null)
			{
				tab = AchievementTabHelper.MainTab;
			}
			uniqueHash = def.defName.GetHashCode();
			unlocked = preUnlocked;
			tracker = (TrackerBase)Activator.CreateInstance(def.tracker.GetType(), new object[] { def.tracker });
			tracker.cardAssigned = this.def.defName;
		}

		public bool BadTex { get; set; }

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
				DefDatabase<SoundDef>.GetNamed("LetterArrive_Good").PlayOneShotOnCamera();
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

			if (!unlocked && !string.IsNullOrEmpty(tracker.PercentComplete.text) && tracker.PercentComplete.percent >= 0)
			{
				float height = iconRect.height * (1 / 8f);
				float width = iconRect.width * (3 / 4f);
				Rect barRect = new Rect(iconRect.x + (iconRect.width / 2) - (width / 2), iconRect.y + iconRect.height - height - (MainTabWindow_Achievements.SpaceBetweenCards / 2f), width, height);
				Widgets.FillableBar(barRect, tracker.PercentComplete.percent, AchievementTex.BarFullTexHor, AchievementTex.DefaultBarBgTex, true);
				Widgets.Label(barRect, tracker.PercentComplete.text);
			}

			Rect labelRect = new Rect(iconRect.x, iconRect.y + iconRect.height, iconRect.width, rect.height - MainTabWindow_Achievements.SpaceBetweenCards - iconRect.height);
			Widgets.Label(labelRect, def.label);

			var font = Text.Font;
			var textColor = GUI.color;
			Text.Font = GameFont.Tiny;
			GUI.color = MainTabWindow_Achievements.LightGray;

			Rect descRect = new Rect(iconRect.x, labelRect.y + MainTabWindow_Achievements.SpaceBetweenCards * 6, iconRect.width, labelRect.height);
			Widgets.Label(descRect, def.description);

			var pointTextSize = Text.CalcSize(def.points.ToString());
			Rect pointRect = new Rect(iconRect.x + pointTextSize.y, descRect.y - MainTabWindow_Achievements.SpaceBetweenCards * 2, iconRect.width - pointTextSize.y, labelRect.height);
			Rect pointIconRect = new Rect(iconRect.x + iconRect.width / 2 - pointTextSize.y, descRect.y - MainTabWindow_Achievements.SpaceBetweenCards * 2, pointTextSize.y, pointTextSize.y);
			Widgets.Label(pointRect, def.points.ToStringSafe());
			GUI.DrawTexture(pointIconRect, AchievementTex.PointsIcon);

			GUI.color = Color.gray;
			var timeSize = Text.CalcSize(dateUnlocked);
			Rect unlockTimeRect = new Rect(iconRect.x, iconRect.y + rect.height - (timeSize.y * 1.5f), iconRect.width, timeSize.y);
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
			Scribe_Deep.Look(ref tracker, "tracker");

			Scribe_Values.Look(ref unlocked, "unlocked", false);
			Scribe_Values.Look(ref devModeUnlocked, "devModeUnlocked", false);
			Scribe_Values.Look(ref dateUnlocked, "dateUnlocked", "Locked");

			Scribe_Values.Look(ref uniqueHash, "uniqueHash");

			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				tab = def.tab ?? AchievementTabHelper.MainTab;
			}
		}
	}
}
