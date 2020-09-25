﻿using System;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;
using OpCodes = System.Reflection.Emit.OpCodes;

namespace AchievementsExpanded
{
    [StaticConstructorOnStartup]
    internal static class AchievementHarmony
    {
        static AchievementHarmony()
        {
            AchievementPointManager.OnStartUp();
            var harmony = new Harmony("smashphil.achievements");

            /// <summary>
            /// Automated Patches by allowing user to specify MethodInfo. 
            /// Solely for organization regarding Trackers
            /// </summary>
            foreach(TrackerBase tracker in AchievementPointManager.TrackersGenerated)
            {
                if (tracker.MethodHook != null && tracker.PatchMethod != null)
                {
                    switch(tracker.PatchType)
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
                        case PatchType.Reverse:
                            throw new NotImplementedException();
                        case PatchType.Unpatch:
                            throw new NotImplementedException();
                    }
                    
                }
            }

            harmony.Patch(original: AccessTools.Method(typeof(TickManager), nameof(TickManager.DoSingleTick)),
                postfix: new HarmonyMethod(typeof(AchievementHarmony),
                nameof(SingleLongTickTracker)));
        }

        public static void PawnJoinedFaction(Pawn p, PopAdaptationEvent ev)
        {
            foreach(var card in AchievementPointManager.AchievementList.Where(a => a.tracker.GetType() == typeof(RaceDefTracker) && !a.unlocked))
            {
                if((card.tracker as RaceDefTracker).Trigger(p.kindDef))
                {
                    card.UnlockCard();
                }
            }
        }

        public static void TimeTickPassed()
        {
            if (Find.TickManager.TicksGame % 2000 == 0)
            {
                foreach(var card in AchievementPointManager.AchievementList.Where(a => a.tracker.GetType().SameOrSubclass(typeof(TimeTracker)) && !a.unlocked))
                {
                    if ((card.tracker as TimeTracker).Trigger())
                    {
                        card.UnlockCard();
                    }
                }
            }
        }

        public static void IncidentTriggered(IncidentParms parms, IncidentWorker __instance, ref bool __result)
        {
            foreach(var card in AchievementPointManager.AchievementList.Where(a => a.tracker.GetType().SameOrSubclass(typeof(IncidentTracker)) && !a.unlocked))
            {
                if (__result && (card.tracker as IncidentTracker).Trigger(__instance.def, parms.target as Map))
                {
                    card.UnlockCard();
                }
            }
        }

        public static void AnimalBondedEvent(Pawn humanlike, Pawn animal, float baseChance, ref bool __result)
        {
            if(__result)
            {
                foreach(var card in AchievementPointManager.AchievementList.Where(a => a.tracker.GetType().SameOrSubclass(typeof(AnimalBondedTracker)) && !a.unlocked))
                {
                    if ((card.tracker as AnimalBondedTracker).Trigger(animal.kindDef))
                    {
                        card.UnlockCard();
                    }
                }
            }
        }

        public static void DevModeToggled(bool value)
        {
            foreach(var card in AchievementPointManager.AchievementList.Where(a => a.tracker.GetType().SameOrSubclass(typeof(DevModeTracker)) && !a.unlocked))
            {
                if ((card.tracker as DevModeTracker).Trigger(value))
                {
                    card.UnlockCard();
                }
            }
        }

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

        public static void KillPawn(DamageInfo? dinfo, Pawn __instance, Hediff exactCulprit = null)
        {
            foreach(var card in AchievementPointManager.AchievementList.Where(a => a.tracker.GetType().SameOrSubclass(typeof(KillTracker)) && !a.unlocked))
            {
                if ((card.tracker as KillTracker).Trigger(__instance))
                {
                    card.UnlockCard();
                }
            }
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
            foreach(var card in AchievementPointManager.AchievementList.Where(a => a.tracker.GetType().SameOrSubclass(typeof(ItemCraftTracker)) && !a.unlocked))
            {
                foreach (Thing thing in things)
                {
                    if ((card.tracker as ItemCraftTracker).Trigger(thing))
                    {
                        card.UnlockCard();
                    }
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
            foreach(var card in AchievementPointManager.AchievementList.Where(a => a.tracker.GetType().SameOrSubclass(typeof(QuestTracker)) && !a.unlocked))
            {
                if ((card.tracker as QuestTracker).Trigger(__instance, outcome))
                {
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
            foreach(var card in AchievementPointManager.AchievementList.Where(a => a.tracker.GetType().SameOrSubclass(typeof(MoodTracker)) && !a.unlocked))
            {
                if ((card.tracker as MoodTracker).Trigger(__result))
                {
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
            foreach(var card in AchievementPointManager.AchievementList.Where(a => a.tracker.GetType().SameOrSubclass(typeof(ResearchTracker)) && !a.unlocked))
            {
                if ((card.tracker as ResearchTracker).Trigger(proj))
                {
                    card.UnlockCard();
                }
            }
        }

        /// <summary>
        /// Building SpawnSetup event
        /// </summary>
        /// <param name="map"></param>
        /// <param name="respawningAfterLoad"></param>
        public static void BuildingSpawned(Pawn worker, Frame __instance)
        {
            foreach(var card in AchievementPointManager.AchievementList.Where(a => a.tracker.GetType().SameOrSubclass(typeof(BuildingTracker)) && !a.unlocked))
            {
                if (worker.Faction == Faction.OfPlayer && (card.tracker as BuildingTracker).Trigger(__instance.def.entityDefToBuild, __instance.Stuff))
                {
                    card.UnlockCard();
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
                foreach(var card in AchievementPointManager.AchievementList.Where(a => a.tracker.GetType().SameOrSubclass(typeof(TraderTracker)) && !a.unlocked))
                {
                    if ((card.tracker as TraderTracker).Trigger(___tradeables))
                    {
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
            foreach(var card in AchievementPointManager.AchievementList.Where(a => a.tracker.GetType().SameOrSubclass(typeof(HediffTracker)) && !a.unlocked))
            {
                if ((card.tracker as HediffTracker).Trigger(hediff))
                {
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
            foreach(var card in AchievementPointManager.AchievementList.Where(a => a.tracker.GetType().SameOrSubclass(typeof(ImmunityHediffTracker)) && !a.unlocked))
            {
                if ((card.tracker as ImmunityHediffTracker).Trigger(diseaseInstance, __instance.immunity))
                {
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
        public static void HediffDeathEvent(Hediff __instance)
        {
            foreach (var card in AchievementPointManager.AchievementList.Where(a => a.tracker.GetType().SameOrSubclass(typeof(HediffDeathTracker)) && !a.unlocked))
            {
                if ((card.tracker as HediffDeathTracker).Trigger(__instance))
                {
                    card.UnlockCard();
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
                foreach (var card in AchievementPointManager.AchievementList.Where(a => a.tracker.GetType().SameOrSubclass(typeof(SettlementDefeatTracker)) && !a.unlocked))
                {
                    if ((card.tracker as SettlementDefeatTracker).Trigger(Find.World.worldObjects.SettlementAt(map.Tile)))
                    {
                        card.UnlockCard();
                    }
                }
            }
        }

        /// <summary>
        /// Hook onto LongTick for Trackers that need constant checking
        /// </summary>
        public static void SingleLongTickTracker()
        {
            if (Find.TickManager.TicksGame % 2000 == 0)
            {
                foreach(var card in AchievementPointManager.AchievementList.Where(a => a.tracker.AttachToLongTick != null && !a.unlocked))
                {
                    if(card.tracker.AttachToLongTick())
                    {
                        card.UnlockCard();
                    }
                }
            }
        }
    }
}
