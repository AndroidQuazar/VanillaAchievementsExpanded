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
		private const float CardSize = 200;
		public const float SpaceBetweenCards = 10;
		private const float CardSizeToResolutionRatio = 9.6f;
		private const float TextAreaHeight = 0.15f;
		private const float SidePanelMargin = 0.05f;
		private const float SidePanelRatio = 0.8f;
		private const float ScreenHeightPercent = 0.8f;
		//Default sizing of side panel based on 1920x1080 resolution ratio
		private const float SidePanelWidth = 1920 * (1 - SidePanelRatio);

		private static float FullPanelWidth;
		public static int CardsPerRow;

		private AchievementPointManager apmCache;
		private List<AchievementReward> rewardCache;
		private static AchievementTabDef curTab;
		private List<TabRecord> tabs = new List<TabRecord>();

		private static Vector2 menuScrollPosition;
		private static Vector2 sidebarScrollPosition;
		private static string searchText;

		public static Color LightGray = new Color(0.85f, 0.85f, 0.85f, 1f);
		public static Color MediumGray = new Color(0.75f, 0.75f, 0.75f, 1f);

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

		public List<AchievementReward> Rewards
		{
			get
			{
				if (rewardCache.NullOrEmpty())
				{
					rewardCache = DefDatabase<AchievementReward>.AllDefsListForReading.Where(r => (CurTab == AchievementTabHelper.MainTab && r.tab is null) 
																							|| r.tab == CurTab).OrderBy(d => d.cost).ToList();
					if (rewardCache.NullOrEmpty())
					{
						rewardCache = DefDatabase<AchievementReward>.AllDefsListForReading.Where(r => r.tab is null || r.tab == AchievementTabHelper.MainTab).OrderBy(d => d.cost).ToList();
					}
				}
				return rewardCache;
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
				sidebarScrollPosition = Vector2.zero;
				curTab = value;
				ClearRewardCache();
			}
		}

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(UI.screenWidth, UI.screenHeight * ScreenHeightPercent);
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
			{
				CurTab = AchievementTabHelper.MainTab;
			}
		}

		public override void PreOpen()
		{
			base.PreOpen();
			FullPanelWidth = Verse.UI.screenWidth - SidePanelWidth;
			float cardMenuWidth = FullPanelWidth - 20f;
			CardsPerRow = Mathf.FloorToInt((cardMenuWidth - (SpaceBetweenCards * 6)) / CardSize);
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect menuRect = inRect;
			menuRect.y += 40f;
			menuRect.height -= 40f;
			menuRect.width = FullPanelWidth;
			DrawAchievementsMenu(menuRect);

			float panelWidth = SidePanelWidth;
			float marginWidth = panelWidth * SidePanelMargin;
			Rect panelRect = new Rect(UI.screenWidth - panelWidth + marginWidth, 0f, panelWidth - marginWidth * 2.5f, inRect.height);
			DrawSidePanel(panelRect);

			Rect cardMenuRect = menuRect.ContractedBy(10f);
			DrawCardWindow(cardMenuRect);
		}

		private void DrawAchievementsMenu(Rect rect)
		{
			Widgets.DrawMenuSection(rect);
			TabDrawer.DrawTabs(rect, tabs, 200f);
			Rect searchRect = new Rect(rect.width - 240, rect.y + 15f, 200f, 30f);
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

			var unlockCountTotal = $"{APM.achievementList.Where(a => a.unlocked).Count()} / {APM.achievementList.Count}";
			Widgets.Label(rect, "AchievementsTotalUnlocked".Translate(unlockCountTotal));

			rect.y += 60;
			Text.Font = GameFont.Medium;
			Widgets.Label(rect, "PointsRedeem".Translate());

			rect.y += 35;
			iconRect.width = rect.width * 0.1f;
			labelRect.width = rect.width * 0.2f;
			var color = GUI.color;

			Rect buttonContainerRect = new Rect(iconRect.x, rect.y, rect.width - 10, rect.height);
			var height = rect.y + Mathf.CeilToInt(Rewards.Count) * 35;
			Rect buttonViewRect = new Rect(buttonContainerRect.x, buttonContainerRect.y, buttonContainerRect.width, height);

			Widgets.BeginScrollView(buttonContainerRect, ref sidebarScrollPosition, buttonViewRect);
			foreach (AchievementReward reward in Rewards.OrderBy(r => r.cost))
			{
				iconRect.y = rect.y;
				labelRect.y = rect.y;
				bool disabled = !string.IsNullOrEmpty(reward.Disabled);
				Rect buttonRect = new Rect(iconRect.x + 90, rect.y, rect.width - (iconRect.width + labelRect.width) - 20, 30);
				Text.Font = GameFont.Medium;
				Widgets.DrawTextureFitted(iconRect, AchievementTex.PointsIcon, 1f);
				Widgets.Label(labelRect, reward.cost.ToString());
				Text.Font = GameFont.Small;
				if (disabled)
				{
					GUI.color = Color.gray;
					TooltipHandler.TipRegion(buttonRect, reward.Disabled);
				}
				if (Widgets.ButtonText(buttonRect, reward.label) && !disabled)
				{
					if (reward.PurchaseReward())
					{
						if (!reward.TryExecuteEvent())
						{
							reward.RefundPoints();
						}
						else
						{
							Close(false);
						}
					}
				}
				rect.y += 35f;
				GUI.color = color;
			}
			Widgets.EndScrollView();
			Text.Font = font;
		}

		private void DrawCardWindow(Rect rect)
		{
			float iconWidth = (rect.width / CardsPerRow) - SpaceBetweenCards - (SpaceBetweenCards * 2 / CardsPerRow);
			float iconHeight = iconWidth + iconWidth * 0.55f;

			var achievementList = APM.achievementList.Where(a => a.tab == CurTab && 
				(string.IsNullOrEmpty(searchText) || a.def.label.Contains(searchText, StringComparison.OrdinalIgnoreCase) || a.def.description.Contains(searchText, StringComparison.OrdinalIgnoreCase)))
				.OrderBy(c => c.def.order).ToList();

			Rect windowRect = new Rect(rect.x, rect.y + 60, rect.width, rect.height);

			Rect cardRect = new Rect(windowRect.x, windowRect.y, iconWidth, iconHeight);
			var height = rect.y + Mathf.CeilToInt(achievementList.Count / CardsPerRow) * (iconHeight + SpaceBetweenCards) + ((achievementList.Count % CardsPerRow == 0) ? 0 : (iconHeight + SpaceBetweenCards));

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
				card.DrawCard(cardRect);
			}

			Widgets.EndScrollView();
		}

		private void ClearRewardCache()
		{
			if (rewardCache is null)
			{
				rewardCache = new List<AchievementReward>();
			}
			else
			{
				rewardCache.Clear();
			}
		}
	}
}
