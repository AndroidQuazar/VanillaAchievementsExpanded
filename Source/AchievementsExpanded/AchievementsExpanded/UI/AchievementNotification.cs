using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace AchievementsExpanded
{
	/// <summary>
	/// Notification class structured similarly to Verse.Window to handle moving dialogs
	/// </summary>
	public class AchievementNotification : MovingWindow
	{
		protected override Vector2 MaxDrift => new Vector2(250, 110);
		protected override Vector2 FloatSpeed => new Vector2(0, -1);
		protected override Vector2 WindowPosition => new Vector2(UI.screenWidth - MaxDrift.x, UI.screenHeight);
		protected override float Margin => 0f;
		protected override int TicksTillRemoval => 1500;

		public AchievementNotification(AchievementCard card, Action clickAction = null)
		{
			this.card = card;
			driftOut = true;
			preventCameraMotion = false;
			this.clickAction = clickAction;
		}

		public override void DoWindowContents(Rect inRect)
		{
			var font = Text.Font;
			base.DoWindowContents(inRect);
			DrawCard(inRect.ContractedBy(10));
			Text.Font = font;
		}

		public override void OnAcceptKeyPressed() { }

		protected virtual void DrawCard(Rect rect)
		{
			Rect imageRect = new Rect(rect.x, rect.y, rect.height, rect.height);
			GUI.DrawTexture(imageRect, AchievementTex.CardBG);
			GUI.DrawTexture(imageRect, card.AchievementIcon);

			var labelWidth = rect.width - (imageRect.width + Padding);
			var labelStart = imageRect.x + imageRect.width + Padding;

			var font1 = Text.Font;
			var anchor = Text.Anchor;
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperCenter;

			string text = "AchievementUnlocked".Translate();
			var textSize = Text.CalcSize(text);
			var textWidth = Mathf.Clamp(textSize.x, 0f, labelWidth);
			Rect labelRect = new Rect(labelStart, imageRect.y, labelWidth, rect.height);
			Widgets.Label(labelRect, text);

			Text.Font = GameFont.Tiny;

			var color = GUI.color;
			GUI.color = MainTabWindow_Achievements.LightGray;
			var titleSize = Text.CalcSize(card.def.label);
			var titleWidth = Mathf.Clamp(titleSize.x, 0f, labelWidth);
			Rect titleRect = new Rect(labelStart, labelRect.y + textSize.y, labelWidth, rect.height);
			Widgets.Label(titleRect, card.def.label);

			GUI.color = MainTabWindow_Achievements.MediumGray;
			var descSize = Text.CalcSize(card.def.description);
			var descWidth = Mathf.Clamp(descSize.x, 0f, labelWidth);
			Rect descRect = new Rect(labelStart, titleRect.y + titleSize.y / 1.5f, labelWidth, rect.height);
			Widgets.Label(descRect, card.def.description);
			
			Text.Font = font1;
			Text.Anchor = anchor;
			GUI.color = color;
		}

		private AchievementCard card;
		private const float Padding = 5f;
	}
}
