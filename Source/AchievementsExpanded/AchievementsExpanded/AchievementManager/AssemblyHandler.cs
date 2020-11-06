using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using HarmonyLib;

namespace AchievementsExpanded
{
    internal class AssemblyHandler : Mod
    {
        private const string AssemblyName = "AchievementsExpanded";

        public AssemblyHandler(ModContentPack content) : base(content)
        {
            var harmony = new Harmony(AchievementHarmony.modIdentifier + "_preloaded");

            EnsureNoDuplicateHashes(harmony);
        }

        internal static void EnsureNoDuplicateHashes(Harmony harmony)
        {
            if (ModsConfig.IsActive(AchievementHarmony.modIdentifier))
            {
                Log.Message($"[{AchievementPointManager.AchievementTag}] Removing duplicate Def Types.");
            }
            else
            {
                Log.Message($"[{AchievementPointManager.AchievementTag}] Mod is not in active mod list. The base mod must be included in order to generate achievements.");
            }

            achievementDefTypes = new HashSet<Type>();
            harmony.Patch(original: AccessTools.Method(typeof(GenDefDatabase), nameof(GenDefDatabase.AllDefTypesWithDatabases)),
                postfix: new HarmonyMethod(typeof(AssemblyHandler),
                nameof(DuplicateDefTypesPassthrough)));
        }

        private static IEnumerable<Type> DuplicateDefTypesPassthrough(IEnumerable<Type> __result)
        {
            IEnumerator<Type> enumerator = __result.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var element = enumerator.Current;
                if (YieldAchievementDefType(element))
                {
                    yield return element;
                }
            }
        }

        private static bool YieldAchievementDefType(Type type)
        {
            if (GetAssemblyNameContainingType(type) == AssemblyName)
            {
                if (!achievementDefTypes.Contains(type) && type.IsSubclassOf(typeof(TrackerBase)) && !type.IsAbstract)
                {
                    achievementDefTypes.Add(type);
                    return true;
                }
                return false;
            }
            return true;
        }

        public static string GetAssemblyNameContainingType(Type type) => Assembly.GetAssembly(type).GetName().Name;

        internal static HashSet<Type> achievementDefTypes = new HashSet<Type>();
    }
}
