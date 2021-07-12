using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using RimWorld;
using RimWorld.Planet;
using HarmonyLib;
using UnityEngine;
using OpCodes = System.Reflection.Emit.OpCodes;

namespace AchievementsExpanded
{
	[StaticConstructorOnStartup]
	internal static class AchievementHarmony
	{
		internal static string modIdentifier = "vanillaexpanded.achievements";
		internal static Dictionary<MethodInfo, HashSet<MethodInfo>> hookToPatchMap = new Dictionary<MethodInfo, HashSet<MethodInfo>>();

		internal static ModMetaData AchievementsMMD;

		public static string CurrentVersion { get; private set; }

		internal static string VersionDir => Path.Combine(AchievementsMMD.RootDir.FullName, "Version.txt");

		static AchievementHarmony()
		{
			AchievementsMMD = ModLister.GetActiveModWithIdentifier(modIdentifier);

			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			CurrentVersion = $"{version.Major}.{version.Minor}.{version.Build}";
			Log.Message($"{AchievementPointManager.AchievementTag} version {CurrentVersion}");

			AchievementPointManager.OnStartUp();
			var harmony = new Harmony(modIdentifier);

			if (UtilityMethods.BaseModActive)
			{
				File.WriteAllText(VersionDir, CurrentVersion);

				harmony.PatchAll();
				/// <summary>
				/// Automated Patches by allowing user to specify MethodInfo. 
				/// Solely for organization regarding Trackers
				/// </summary>
				foreach(TrackerBase tracker in AchievementPointManager.TrackersGenerated)
				{
					if (tracker.MethodHook != null && tracker.PatchMethod != null)
					{
						if (TryRegisterPatch(tracker.MethodHook, tracker.PatchMethod))
						{
							switch (tracker.PatchType)
							{
								case PatchType.Prefix:
									harmony.Patch(original: tracker.MethodHook,
										prefix: new HarmonyMethod(tracker.PatchMethod));
									break;
								case PatchType.Postfix:
									harmony.Patch(original: tracker.MethodHook,
										postfix: new HarmonyMethod(tracker.PatchMethod));
									break;
								case PatchType.Transpiler:
									harmony.Patch(original: tracker.MethodHook,
										transpiler: new HarmonyMethod(tracker.PatchMethod));
									break;
								case PatchType.Finalizer:
									harmony.Patch(original: tracker.MethodHook,
										finalizer: new HarmonyMethod(tracker.PatchMethod));
									break;
							}
						}
					}
				}

				/* Additional Event Catches */
				harmony.Patch(original: AccessTools.Method(typeof(Thing), nameof(Thing.Kill)),
					prefix: new HarmonyMethod(typeof(AchievementHarmony),
					nameof(KillThing)));
				harmony.Patch(original: AccessTools.Method(typeof(Pawn_RecordsTracker), nameof(Pawn_RecordsTracker.AddTo)),
					postfix: new HarmonyMethod(typeof(AchievementHarmony),
					nameof(RecordAddToEvent)));

				/* Event thrown every Long Tick */
				harmony.Patch(original: AccessTools.Method(typeof(TickManager), nameof(TickManager.DoSingleTick)),
					postfix: new HarmonyMethod(typeof(AchievementHarmony),
					nameof(SingleLongTickTracker)));

				/* Debug Actions register in menu */
				harmony.Patch(original: AccessTools.Method(typeof(Dialog_DebugActionsMenu), "GenerateCacheForMethod"),
					prefix: new HarmonyMethod(typeof(DebugActionsSetup), 
					nameof(DebugActionsSetup.GenerateCacheForVAEDebugActions)));
				harmony.Patch(original: AccessTools.Constructor(typeof(Dialog_DebugActionsMenu)),
					prefix: new HarmonyMethod(typeof(DebugActionsSetup),
					nameof(DebugActionsSetup.ClearCachedActions)));

				/* Debugging */
				//harmony.Patch(original: AccessTools.Method(typeof(DebugLoadIDsSavingErrorsChecker), nameof(DebugLoadIDsSavingErrorsChecker.RegisterDeepSaved)),
				//    prefix: new HarmonyMethod(typeof(AchievementHarmony),
				//    nameof(DebugTest)));
			}
		}

		/// <summary>
		/// A method that checks whether or not a method has already been patched, in order to prevent duplicate patching.
		/// </summary>
		/// <param name="methodHook">The method that is being patched.</param>
		/// <param name="patchMethod">The patch method (prefix/postfix/transpiler/finalizer).</param>
		/// <param name="register">If true, the patch will be 'registered'. This does not mean that the patch is applied, the caller should do that themselves.</param>
		/// <returns>True if the method has not already been patched by the target method, false otherwise.</returns>
		internal static bool TryRegisterPatch(MethodInfo methodHook, MethodInfo patchMethod)
		{
			if (hookToPatchMap.TryGetValue(methodHook, out var hashSet))
			{
				return hashSet.Add(patchMethod);
			}
			hashSet = new HashSet<MethodInfo>() { patchMethod };
			hookToPatchMap.Add(methodHook, hashSet);
			return true;
		}

		public static void DebugTest()
		{
		}

		/// <summary>
		/// Pawn PopulationAdaptation event
		/// </summary>
		/// <param name="p"></param>
		/// <param name="ev"></param>
		public static void PawnJoinedFaction(Pawn p, PopAdaptationEvent ev)
		{
			foreach(var card in AchievementPointManager.GetCards<PawnJoinedTracker>())
			{
				try
				{
					if (ev == PopAdaptationEvent.GainedColonist && (card.tracker as PawnJoinedTracker).Trigger(p))
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
		/// Any incident being triggered event
		/// </summary>
		/// <param name="parms"></param>
		/// <param name="__instance"></param>
		/// <param name="__result"></param>
		public static void IncidentTriggered(IncidentParms parms, IncidentWorker __instance, ref bool __result)
		{
			foreach(var card in AchievementPointManager.GetCards<IncidentTracker>())
			{
				try
				{
					if (__result && (card.tracker as IncidentTracker).Trigger(__instance.def, parms.target as Map))
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
		/// Animal being set to Bonded event
		/// </summary>
		/// <param name="humanlike"></param>
		/// <param name="animal"></param>
		/// <param name="baseChance"></param>
		/// <param name="__result"></param>
		public static void AnimalBondedEvent(Pawn humanlike, Pawn animal, float baseChance, ref bool __result)
		{
			if(__result)
			{
				foreach(var card in AchievementPointManager.GetCards<AnimalBondedTracker>())
				{
					try
					{
						if ((card.tracker as AnimalBondedTracker).Trigger(animal.kindDef))
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
		}

		/// <summary>
		/// DevMode event (when devMode is toggled)
		/// </summary>
		/// <param name="value"></param>
		public static void DevModeToggled(bool value)
		{
			foreach(var card in AchievementPointManager.GetCards<DevModeTracker>())
			{
				try
				{
					if ((card.tracker as DevModeTracker).Trigger(value))
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
		/// MentalState event
		/// </summary>
		/// <param name="instructions"></param>
		/// <returns></returns>
		public static IEnumerable<CodeInstruction> MentalBreakTriggered(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> instructionList = instructions.ToList();

			//register all but Return statement at end (duplicate and check Worker.TryStart return value)
			for (int i = 0; i < instructionList.Count - 1; i++)
			{
				CodeInstruction instruction = instructionList[i];

				yield return instruction;
			}

			yield return new CodeInstruction(opcode: OpCodes.Dup);
			yield return new CodeInstruction(opcode: OpCodes.Ldloc_0);
			yield return new CodeInstruction(opcode: OpCodes.Call, operand: AccessTools.Method(typeof(UtilityMethods), nameof(UtilityMethods.MentalBreakTrigger)));

			yield return new CodeInstruction(opcode: OpCodes.Ret);
		}

		/// <summary>
		/// Pawn Kill event triggered whenever a pawn is killed
		/// </summary>
		/// <param name="dinfo"></param>
		/// <param name="__instance"></param>
		/// <param name="exactCulprit"></param>
		public static void KillPawn(DamageInfo? dinfo, Pawn __instance, Hediff exactCulprit = null)
		{
			foreach(var card in AchievementPointManager.GetCards<KillTracker>())
			{
				try
				{
					if ((card.tracker as KillTracker).Trigger(__instance, dinfo))
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
		/// Additional Event where Thing is killed. Catches Things that are Pawns calling here instead of main Pawn Kill method. (DebugGeneral for instance)
		/// </summary>
		/// <param name="dinfo"></param>
		/// <param name="__instance"></param>
		/// <param name="exactCulprit"></param>
		public static bool KillThing(DamageInfo? dinfo, Thing __instance, Hediff exactCulprit = null)
		{
			if (__instance is Pawn pawn)
			{
				pawn.Kill(dinfo, exactCulprit);
				return false;
			}
			return true;
		}

		/// <summary>
		/// Attaches to QuestManager Event triggered upon making an item from a Bill
		/// </summary>
		/// <param name="newThing"></param>
		/// <param name="loc"></param>
		/// <param name="map"></param>
		/// <param name="rot"></param>
		/// <param name="wipeMode"></param>
		/// <param name="respawningAfterLoad"></param>
		/// <notes>Cannot Transpile onto Toil itself, delegate initAction gets inlined</notes>
		public static void ThingSpawned(Pawn worker, List<Thing> things)
		{
			foreach(var card in AchievementPointManager.GetCards<ItemCraftTracker>())
			{
				try
				{
					foreach (Thing thing in things)
					{
						if ((card.tracker as ItemCraftTracker).Trigger(thing))
						{
							card.UnlockCard();
						}
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
		/// Attaches to Quest End method to capture both quest and outcome
		/// </summary>
		/// <param name="outcome"></param>
		/// <param name="__instance"></param>
		/// <param name="sendLetter"></param>
		public static void QuestEnded(QuestEndOutcome outcome, Quest __instance, bool sendLetter = true)
		{
			foreach(var card in AchievementPointManager.GetCards<QuestTracker>())
			{
				try
				{
					if ((card.tracker as QuestTracker).Trigger(__instance, outcome))
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
		/// Inserts right before MoteThrow on Levelup to trigger Tracker event
		/// </summary>
		/// <param name="instructions"></param>
		/// <returns></returns>
		public static IEnumerable<CodeInstruction> LevelUpMoteHook(IEnumerable<CodeInstruction> instructions)
		{
			foreach (CodeInstruction instruction in instructions)
			{
				if (instruction.Calls(AccessTools.Method(typeof(MoteMaker), nameof(MoteMaker.ThrowText), new Type[] { typeof(Vector3), typeof(Map), typeof(string), typeof(float) })))
				{
					yield return new CodeInstruction(opcode: OpCodes.Ldarg_0);
					yield return new CodeInstruction(opcode: OpCodes.Ldfld, operand: AccessTools.Field(typeof(SkillRecord), nameof(SkillRecord.def)));
					yield return new CodeInstruction(opcode: OpCodes.Ldarg_0);
					yield return new CodeInstruction(opcode: OpCodes.Ldfld, operand: AccessTools.Field(typeof(SkillRecord), nameof(SkillRecord.levelInt)));
					yield return new CodeInstruction(opcode: OpCodes.Call, operand: AccessTools.Method(typeof(UtilityMethods), nameof(UtilityMethods.LevelUpTrigger)));
				}
				yield return instruction;
			}
		}
		
		/// <summary>
		/// Grab average mood of colony from History event which triggers every 60000 ticks
		/// </summary>
		/// <param name="__result"></param>
		public static void AverageMoodColony(float __result)
		{
			foreach(var card in AchievementPointManager.GetCards<MoodTracker>())
			{
				try
				{
					if ((card.tracker as MoodTracker).Trigger(__result))
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
		/// ResearchProject event when project is finished
		/// </summary>
		/// <param name="proj"></param>
		/// <param name="doCompletionDialog"></param>
		/// <param name="researcher"></param>
		public static void ResearchProjectFinished(ResearchProjectDef proj, bool doCompletionDialog, Pawn researcher)
		{
			foreach(var card in AchievementPointManager.GetCards<ResearchTracker>())
			{
				try
				{
					if ((card.tracker as ResearchTracker).Trigger(proj))
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
		/// Spawn event for Buildings
		/// </summary>
		/// <param name="newThing"></param>
		/// <param name="loc"></param>
		/// <param name="map"></param>
		/// <param name="rot"></param>
		/// <param name="wipeMode"></param>
		/// <param name="respawningAfterLoad"></param>
		public static void ThingBuildingSpawned(Thing newThing, IntVec3 loc, Map map, Rot4 rot, WipeMode wipeMode = WipeMode.Vanish, bool respawningAfterLoad = false)
		{
			if (newThing is Building building && building.Faction == Faction.OfPlayer && Current.ProgramState == ProgramState.Playing)
			{
				foreach(var card in AchievementPointManager.GetCards<BuildingTracker>())
				{
					try
					{
						if ((card.tracker as BuildingTracker).Trigger(building))
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
		}

		/// <summary>
		/// TradeDeal event to retrieve what was exchanged
		/// </summary>
		/// <param name="actuallyTraded"></param>
		/// <param name="__result"></param>
		/// <param name="___tradeables"></param>
		public static void TradeDealComplete(bool __result, List<Tradeable> ___tradeables)
		{
			if (__result)
			{
				foreach(var card in AchievementPointManager.GetCards<TraderTracker>())
				{
					try
					{
						if ((card.tracker as TraderTracker).Trigger(___tradeables))
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
		}
		
		/// <summary>
		/// Event whenever a Hediff gets added to a pawn
		/// </summary>
		/// <param name="dinfo"></param>
		public static void HediffAdded(Hediff hediff, BodyPartRecord part = null, DamageInfo? dinfo = null, DamageWorker.DamageResult result = null)
		{
			foreach(var card in AchievementPointManager.GetCards<HediffTracker>())
			{
				try
				{
					if ((card.tracker as HediffTracker).Trigger(hediff))
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
		/// Event on immunity ticker
		/// </summary>
		/// <param name="pawn"></param>
		/// <param name="sick"></param>
		/// <param name="diseaseInstance"></param>
		public static void ImmunityTicking(Pawn pawn, bool sick, Hediff diseaseInstance, ImmunityRecord __instance)
		{
			foreach(var card in AchievementPointManager.GetCards<ImmunityHediffTracker>())
			{
				try
				{
					if ((card.tracker as ImmunityHediffTracker).Trigger(diseaseInstance, __instance.immunity))
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
		/// Hediff PawnDeath Event
		/// </summary>
		/// <param name="p"></param>
		/// <param name="ev"></param>
		/// <param name="dinfo"></param>
		public static void HediffDeathEvent(Hediff __instance, ref bool __result)
		{
			if (__result)
			{
				foreach (var card in AchievementPointManager.GetCards<HediffDeathTracker>())
				{
					try
					{
						if ((card.tracker as HediffDeathTracker).Trigger(__instance))
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
		}

		/// <summary>
		/// SettlementDefeated Event
		/// </summary>
		/// <param name="map"></param>
		/// <param name="faction"></param>
		/// <param name="__result"></param>
		/// <remarks>Only trigger on success</remarks>
		public static void SettlementDefeatedEvent(Map map, Faction faction, ref bool __result)
		{
			if(__result)
			{
				foreach (var card in AchievementPointManager.GetCards<SettlementDefeatTracker>())
				{
					try
					{
						if ((card.tracker as SettlementDefeatTracker).Trigger(Find.World.worldObjects.SettlementAt(map.Tile)))
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
		}

		/// <summary>
		/// Increment Event where Records are incremented by 1
		/// </summary>
		/// <param name="def"></param>
		/// <param name="__instance"></param>
		public static void RecordEvent(RecordDef def, Pawn_RecordsTracker __instance)
		{
			foreach (var card in AchievementPointManager.GetCards<RecordEventTracker>())
			{
				try
				{
					if ((card.tracker as RecordEventTracker).Trigger(def, __instance.pawn))
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
		/// Additional Patch to AddTo to catch outside calls to increment the non Time-based RecordDef
		/// </summary>
		/// <param name="def"></param>
		/// <param name="__instance"></param>
		public static void RecordAddToEvent(RecordDef def, Pawn_RecordsTracker __instance)
		{
			RecordEvent(def, __instance);
		}

		/// <summary>
		/// AddTo event where a float value is added to the Record (used for Time based Records)
		/// </summary>
		/// <param name="def"></param>
		/// <param name="value"></param>
		/// <param name="__instance"></param>
		public static IEnumerable<CodeInstruction> RecordTimeEvent(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> instructionList = instructions.ToList();

			for(int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction instruction = instructionList[i];

				if (instruction.opcode == OpCodes.Stloc_3)
				{
					yield return instruction;
					instruction = instructionList[++i];

					yield return new CodeInstruction(opcode: OpCodes.Ldloc_3);
					yield return new CodeInstruction(opcode: OpCodes.Ldarg_0);
					yield return new CodeInstruction(opcode: OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_RecordsTracker), nameof(Pawn_RecordsTracker.pawn)));
					yield return new CodeInstruction(opcode: OpCodes.Ldarg_1);
					yield return new CodeInstruction(opcode: OpCodes.Call, AccessTools.Method(typeof(UtilityMethods), nameof(UtilityMethods.RecordTimeEvent)));
				}
				yield return instruction;
			}
		}

		/// <summary>
		/// Hook onto LongTick for Trackers that need constant checking
		/// </summary>
		public static void SingleLongTickTracker()
		{
			if (Find.TickManager.TicksGame % 2000 == 0)
			{
				foreach (var card in AchievementPointManager.GetLongTickCards().Where(c => !c.unlocked))
				{
					try
					{
						if (card.tracker.AttachToLongTick())
						{
							card.UnlockCard();
						}
					}
					catch (Exception ex)
					{
						Log.ErrorOnce($"Unable to trigger event for card validation. To avoid further errors {card.def.LabelCap} has been automatically unlocked.\n\nException={ex.Message}", card.GetHashCode() ^ 12351231);
						card.UnlockCard();
					}
				}
			}
		}
	}
}
