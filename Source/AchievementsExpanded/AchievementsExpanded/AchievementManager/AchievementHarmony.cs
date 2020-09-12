using System;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using HarmonyLib;
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
