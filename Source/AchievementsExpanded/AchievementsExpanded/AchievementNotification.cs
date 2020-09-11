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
        protected override Vector2 MaxDrift => new Vector2(250, 75);
        protected override Vector2 FloatSpeed => new Vector2(0, -2);
        protected override Vector2 WindowPosition => new Vector2(UI.screenWidth - MaxDrift.x, UI.screenHeight);
        protected override float Margin => 0f;
        protected override int TicksTillRemoval => 800;

        public AchievementNotification(AchievementCard card, Action clickAction = null)
        {
            this.card = card;
            driftOut = true;
            preventCameraMotion = false;
            this.clickAction = clickAction;
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
            DrawCard(inRect.ContractedBy(10));
        }

        public override void OnAcceptKeyPressed() { }
        public override void OnCancelKeyPressed() { }

        protected virtual void DrawCard(Rect rect)
        {
            Rect imageRect = new Rect(rect.x, rect.y, rect.height, rect.height);
            GUI.DrawTexture(imageRect, AchievementTex.CardBG);
            GUI.DrawTexture(imageRect, card.AchievementIcon);

            var labelWidth = rect.width - (imageRect.width + Padding);
            var labelStart = imageRect.x + imageRect.width + Padding + (labelWidth / 2);

            string text = "AchievementUnlocked".Translate();
            var textSize = Text.CalcSize(text);
            Rect labelRect = new Rect(labelStart - (textSize.x / 2), imageRect.y, labelWidth, rect.height);
            Widgets.Label(labelRect, text);

            var color = GUI.color;
            GUI.color = MainTabWindow_Achievements.LightGray;
            var titleSize = Text.CalcSize(card.def.label);
            Rect titleRect = new Rect(labelStart - (titleSize.x / 2), labelRect.y + textSize.y, labelWidth, rect.height);
            Widgets.Label(titleRect, card.def.label);

            var font = Text.Font;
            Text.Font = GameFont.Tiny;

            var descSize = Text.CalcSize(card.def.description);
            Rect descRect = new Rect(labelStart - (descSize.x / 2), titleRect.y + titleSize.y, labelWidth, rect.height);
            Widgets.Label(descRect, card.def.description);

            Text.Font = font;
            GUI.color = color;
        }

        private AchievementCard card;
        private const float Padding = 5f;
    }
}
