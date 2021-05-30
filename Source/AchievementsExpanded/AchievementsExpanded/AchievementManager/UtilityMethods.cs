using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using HarmonyLib;

namespace AchievementsExpanded
{
	/// <summary>
	/// Easier and prettier than implementing IndexOf directly into CardList filter
	/// </summary>
	public static class UtilityMethods
	{
		private static readonly Color InactiveColor = new Color(0.37f, 0.37f, 0.37f, 0.8f);
		private static HashSet<string> activeModsCopiedHashSet;
		private static bool tmpState;

		public static bool BaseModActive
		{
			get
			{
				if (activeModsCopiedHashSet.EnumerableNullOrEmpty())
				{
					activeModsCopiedHashSet = (HashSet<string>)AccessTools.Field(typeof(ModsConfig), "activeModsHashSet").GetValue(null);
				}
				return !string.IsNullOrEmpty(activeModsCopiedHashSet.FirstOrDefault(s => s.Contains(AchievementHarmony.modIdentifier)));
			}
		}

		/// <summary>
		/// Check if string contains another string within
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <param name="comp"></param>
		/// <returns></returns>
		public static bool Contains(this string source, string target, StringComparison comp)
		{
			return source?.IndexOf(target, comp) >= 0;
		}

		/// <summary>
		/// Check if Type is equal to or subclass from target Type
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool SameOrSubclass(this Type source, Type target)
		{
			return source == target || source.IsSubclassOf(target);
		}

		/// <summary>
		/// Helper Method for MentalBreak Transpiler (Check if MentalBreak Initiated -> Trigger on Trackers)
		/// </summary>
		public static void MentalBreakTrigger(bool started, MentalBreakDef def)
		{
			foreach (var card in AchievementPointManager.GetCards<MentalBreakTracker>())
			{
				try
				{
					if (started && (card.tracker as MentalBreakTracker).Trigger(def))
					{
						card.UnlockCard();
					}
				}
				catch (Exception ex)
				{
					Log.Error($"Unable to trigger event for card validation. To avoid further errors {card.def.LabelCap} has been automatically unlocked.\n\nException={ex.Message}");
					card.UnlockCard();
				}
			}
		}

		/// <summary>
		/// Helper Method for UnfinishedThing crafted Transpiler
		/// </summary>
		/// <param name="thing"></param>
		public static void ItemCraftedTrigger(Thing thing)
		{
			foreach(var card in AchievementPointManager.GetCards<ItemCraftTracker>())
			{
				try
				{
					if ((card.tracker as ItemCraftTracker).Trigger(thing))
					{
						card.UnlockCard();
					}
				}
				catch (Exception ex)
				{
					Log.Error($"Unable to trigger event for card validation. To avoid further errors {card.def.LabelCap} has been automatically unlocked.\n\nException={ex.Message}");
					card.UnlockCard();
				}
			}
		}

		/// <summary>
		/// HelperMethod for LevelUp Mote Transpiler
		/// </summary>
		/// <param name="skill"></param>
		public static void LevelUpTrigger(SkillDef skill, int level)
		{
			foreach(var card in AchievementPointManager.GetCards<SkillTracker>())
			{
				try
				{
					if ((card.tracker as SkillTracker).Trigger(skill, level))
					{
						card.UnlockCard();
					}
				}
				catch (Exception ex)
				{
					Log.Error($"Unable to trigger event for card validation. To avoid further errors {card.def.LabelCap} has been automatically unlocked.\n\nException={ex.Message}");
					card.UnlockCard();
				}
			}
		}

		/// <summary>
		/// HelperMethod for Time based record event
		/// </summary>
		/// <param name="def"></param>
		/// <param name="pawn"></param>
		/// <param name="interval"></param>
		public static void RecordTimeEvent(RecordDef def, Pawn pawn, int interval)
		{
			foreach (var card in AchievementPointManager.GetCards<RecordTimeTracker>())
			{
				try
				{
					var tracker = card.tracker as RecordTimeTracker;
					if (tracker.def == def && tracker.Trigger(def, pawn, (float)interval))
					{
						card.UnlockCard();
					}
				}
				catch (Exception ex)
				{
					Log.Error($"Unable to trigger event for card validation. To avoid further errors {card.def.LabelCap} has been automatically unlocked.\n\nException={ex.Message}");
					card.UnlockCard();
				}
			}
		}

		/// <summary>
		/// Get all items from Player's home inventory
		/// </summary>
		/// <param name="thingDef"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static bool PlayerHas(ThingDef thingDef, out int total, int count = 1)
		{
			if (count <= 0)
			{
				total = 0;
				return true;
			}
			int num = 0;
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				num += maps[i].listerThings.ThingsOfDef(thingDef).Sum(t => t.stackCount);
				if (num >= count)
				{
					total = num;
					return true;
				}
			}
			total = num;
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int j = 0; j < caravans.Count; j++)
			{
				if (caravans[j].IsPlayerControlled)
				{
					List<Thing> list = CaravanInventoryUtility.AllInventoryItems(caravans[j]);
					for (int k = 0; k < list.Count; k++)
					{
						if (list[k].def == thingDef)
						{
							num += list[k].stackCount;
							total += num;
							if (num >= count)
							{
								return true;
							}
						}
					}
				}
			}
			return total >= count;
		}

		public static void ConfirmationBoxCheckboxLabeled(this Listing_Standard listing, string label, ref bool checkOn, string tooltip = null)
		{
			float lineHeight = Text.LineHeight;
			Rect rect = listing.GetRect(lineHeight);
			if (!tooltip.NullOrEmpty())
			{
				if (Mouse.IsOver(rect))
				{
					Widgets.DrawHighlight(rect);
				}
				TooltipHandler.TipRegion(rect, tooltip);
			}
			ConfirmationCheckbox(rect, label, ref checkOn, false, null, null, false);
			listing.Gap(listing.verticalSpacing);
		}

		private static void ConfirmationCheckbox(Rect rect, string label, ref bool checkOn, bool disabled = false, Texture2D texChecked = null, Texture2D texUnchecked = null, bool placeCheckboxNearText = false)
		{
			TextAnchor anchor = Text.Anchor;
			Text.Anchor = TextAnchor.MiddleLeft;
			if (placeCheckboxNearText)
			{
				rect.width = Mathf.Min(rect.width, Text.CalcSize(label).x + 24f + 10f);
			}
			Widgets.Label(rect, label);
			if (!disabled && Widgets.ButtonInvisible(rect, true))
			{
				bool checkState = !checkOn;
				tmpState = false;
				if (checkState)
				{
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("DebugWriterConfirmation".Translate(), delegate()
					{
						tmpState = true;
						SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(null);
					}));
				}
			}
			checkOn = tmpState;
			CheckboxDraw(rect.x + rect.width - 24f, rect.y, checkOn, disabled, 24f, null, null);
			Text.Anchor = anchor;
		}

		private static void CheckboxDraw(float x, float y, bool active, bool disabled, float size = 24f, Texture2D texChecked = null, Texture2D texUnchecked = null)
		{
			Color color = GUI.color;
			if (disabled)
			{
				GUI.color = InactiveColor;
			}
			Texture2D image;
			if (active)
			{
				image = ((texChecked != null) ? texChecked : Widgets.CheckboxOnTex);
			}
			else
			{
				image = ((texUnchecked != null) ? texUnchecked : Widgets.CheckboxOffTex);
			}
			GUI.DrawTexture(new Rect(x, y, size, size), image);
			if (disabled)
			{
				GUI.color = color;
			}
		}
	}
}
