using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace AchievementsExpanded
{
    public class MainTabWindow_Achievements : MainTabWindow
    {
        public AchievementPointManager APM
        {
            get
            {
                if (apmCache is null)
                {
                    apmCache = Current.Game.GetComponent<AchievementPointManager>();
                }
                return apmCache;
            }

        }

        private AchievementTabDef CurTab
		{
			get
			{
				return curTab;
			}
			set
			{
				if (value == curTab)
					return;
				curTab = value;
			}
		}

        public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(Verse.UI.screenWidth, Verse.UI.screenHeight * ScreenHeightPercent);
			}
		}

        public override void PostOpen()
		{
			base.PostOpen();
			tabs.Clear();
			foreach (AchievementTabDef localTabDef2 in DefDatabase<AchievementTabDef>.AllDefs)
			{
				AchievementTabDef localTabDef = localTabDef2;
				tabs.Add(new TabRecord(localTabDef.LabelCap, delegate()
				{
					CurTab = localTabDef;
				}, () => CurTab == localTabDef));
			}
            if (CurTab is null)
                CurTab = AchievementTabDefOf.Main;
		}

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
            Rect menuRect = inRect;
            menuRect.y += 40f;
            menuRect.height -= 40f;
            menuRect.width = UI.screenWidth * SidePanelRatio;
            DrawAchievementsMenu(menuRect);

            float panelWidth = UI.screenWidth - menuRect.width;
            float marginWidth = panelWidth * SidePanelMargin;
            float marginRowheight = panelWidth * TextAreaHeight;
            Rect panelRect = new Rect(UI.screenWidth - panelWidth + marginWidth, 0f, panelWidth - marginWidth, marginRowheight);
            DrawSidePanel(panelRect);

            Rect cardMenuRect = menuRect.ContractedBy(10f);
            DrawCardWindow(cardMenuRect);
        }

        private void DrawAchievementsMenu(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            TabDrawer.DrawTabs(rect, tabs, 200f);
            Rect searchRect = new Rect(rect.x + 10f, rect.y + 15f, 200f, 30f);
            searchText = Widgets.TextField(searchRect, searchText);
        }
        
        private void DrawSidePanel(Rect rect)
        {
            var font = Text.Font;
            Text.Font = GameFont.Small;

            Widgets.Label(rect, "SideBarInfo".Translate());

            rect.y += 40;

            Text.Font = GameFont.Medium;
            Widgets.Label(rect, "StorytellerPoints".Translate());

            rect.y += 40;

            Text.Font = GameFont.Small;
            Rect iconRect = new Rect(rect.x, rect.y, 30, 30);
            Rect labelRect = new Rect(rect.x + 40, rect.y + 5f, rect.width, rect.height);
            Widgets.DrawTextureFitted(iconRect, AchievementTex.PointsIcon, 1f);
            Widgets.Label(labelRect, "PointsAvailable".Translate(APM.availablePoints));

            rect.y += 35;
            iconRect.y += 35;
            labelRect.y += 35;
            Widgets.DrawTextureFitted(iconRect, AchievementTex.PointsIconDark, 1f);
            Widgets.Label(labelRect, "PointsEarned".Translate(APM.totalEarnedPoints));

            rect.y += 40;

            Text.Font = GameFont.Tiny;
            var unlockCount = $"{APM.activeAchievements.Where(a => a.unlocked).Count()} / {APM.activeAchievements.Count}";
            Widgets.Label(rect, "AchievementsUnlocked".Translate(unlockCount));

            Text.Font = font;
        }

        private void DrawCardWindow(Rect rect)
        {
            float iconWidth = (rect.width / CardsPerRow) - SpaceBetweenCards - (SpaceBetweenCards * 2 / CardsPerRow);
            float iconHeight = iconWidth + iconWidth * 0.55f;

            var achievementList = APM.activeAchievements.Where(a => a.tab == CurTab && (string.IsNullOrEmpty(searchText) || a.def.label.Contains(searchText, StringComparison.OrdinalIgnoreCase))).ToList();

            Rect windowRect = new Rect(rect.x, rect.y + 60, rect.width, rect.height);

            Rect cardRect = new Rect(windowRect.x, windowRect.y, iconWidth, iconHeight);
            var height = rect.y + Mathf.CeilToInt(achievementList.Count / CardsPerRow) * (iconHeight + SpaceBetweenCards) + (iconHeight + SpaceBetweenCards);

            Rect viewRect = new Rect(windowRect.x, windowRect.y, windowRect.width - SpaceBetweenCards * 2, height);
            
            Widgets.BeginScrollView(windowRect, ref menuScrollPosition, viewRect);

            for (int i = 0; i < achievementList.Count; i++)
            {
                var card = achievementList[i];
                cardRect.x = windowRect.x + (iconWidth + SpaceBetweenCards) * (i % CardsPerRow);
                if (i % CardsPerRow == 0 && i > 0)
                {
                    cardRect.y = windowRect.y + (iconHeight + SpaceBetweenCards) * Mathf.FloorToInt(i / CardsPerRow);
                }
                DrawCard(cardRect, card);
            }

            Widgets.EndScrollView();
        }

        private void DrawCard(Rect rect, AchievementCard card)
        {
            Rect iconRect = new Rect(rect.x, rect.y, rect.width, rect.width).ContractedBy(SpaceBetweenCards);

            Widgets.DrawMenuSection(rect);
            GUI.DrawTexture(iconRect, AchievementTex.CardBG);

            Rect innerIconRect = iconRect.ContractedBy(3);
            var color = GUI.color;
            if(!card.unlocked)
                GUI.color = Color.black;
            GUI.DrawTexture(innerIconRect, card.AchievementIcon);
            GUI.color = color;

            var textWidth = Text.CalcSize(card.def.label).x;
            Rect labelRect = new Rect(rect.x + (rect.width / 2) - (textWidth / 2), iconRect.y + iconRect.width, iconRect.width, rect.height - SpaceBetweenCards - iconRect.height);
            if(textWidth > labelRect.width)
            {
                labelRect.x = iconRect.x;
            }
            Widgets.Label(labelRect, card.def.label);

            var font = Text.Font;
            var textColor = GUI.color;
            Text.Font = GameFont.Tiny;
            GUI.color = LightGray;
            var descTextFull = $"{card.def.points} - {card.def.description}";
            var descSize = Text.CalcSize(descTextFull);
            var descWidth = Mathf.Clamp(descSize.x, 0f, iconRect.width);
            Rect pointIconRect = new Rect(rect.x + (rect.width / 2) - (descWidth / 2) - 10, labelRect.y + SpaceBetweenCards * 5f, descSize.y, descSize.y);
            Rect descRect = new Rect(pointIconRect.x + pointIconRect.width + 2, pointIconRect.y + 1, iconRect.width, labelRect.height);
            GUI.DrawTexture(pointIconRect, AchievementTex.PointsIcon);
            Widgets.Label(descRect, descTextFull);

            GUI.color = Color.gray;
            var timeSize = Text.CalcSize(card.dateUnlocked);
            var timeWidth = Mathf.Clamp(timeSize.x, 0f, iconRect.width);
            Rect unlockTimeRect = new Rect(rect.x + (rect.width / 2) - (timeWidth / 2), iconRect.y + rect.height - (timeSize.y * 1.5f), rect.width, timeSize.y);
            Widgets.Label(unlockTimeRect, card.dateUnlocked);

            Text.Font = font;
            GUI.color = textColor;
        }

        private const int CardsPerRow = 7;

        private const float SpaceBetweenCards = 10;

        private const float TextAreaHeight = 0.15f;
        private const float SidePanelMargin = 0.05f;
        private const float SidePanelRatio = 0.8f;
        private const float ScreenHeightPercent = 0.8f;

        private AchievementPointManager apmCache;
        private static AchievementTabDef curTab;
        private List<TabRecord> tabs = new List<TabRecord>();

        private static Vector2 menuScrollPosition;
        private static string searchText;

        public static Color LightGray = new Color(0.85f, 0.85f, 0.85f, 1f);
    }
}
