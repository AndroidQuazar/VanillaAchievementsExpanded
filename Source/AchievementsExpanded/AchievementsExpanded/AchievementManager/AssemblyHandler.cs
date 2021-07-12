using System;
using System.Reflection;
using System.Diagnostics;
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

		internal static HashSet<Type> achievementDefTypes = new HashSet<Type>();

		public AssemblyHandler(ModContentPack content) : base(content)
		{
			var harmony = new Harmony(AchievementHarmony.modIdentifier + "_preloaded");

			EnsureNoDuplicateHashes(harmony);
		}

		public static string GetAssemblyNameContainingType(Type type) => Assembly.GetAssembly(type).GetName().Name;

		internal static void EnsureNoDuplicateHashes(Harmony harmony)
		{
			if (UtilityMethods.BaseModActive)
			{
				CheckAchievementVersions();
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

		private static void CheckAchievementVersions()
		{
			if (UtilityMethods.BaseModActive)
			{
				try
				{
					bool vaePassed = false;
					Assembly vaeAssembly = Assembly.GetExecutingAssembly();
					string currentVersion = vaeAssembly.GetName().Version.ToString();
					if (int.TryParse(string.Join("", currentVersion.Split('.')), out int version))
					{
						foreach (ModContentPack mod in LoadedModManager.RunningMods)
						{
							if (mod.PackageId.Contains(AchievementHarmony.modIdentifier))
							{
								vaePassed = true;
							}
							else
							{
								for (int i = 0; i < mod.assemblies.loadedAssemblies.Count; i++)
								{
									var modAssembly = mod.assemblies.loadedAssemblies[i];
									if (modAssembly.GetName().Name == vaeAssembly.GetName().Name)
									{
										if (!vaePassed)
										{
											Log.Error($"[{mod.Name}] This mod adds achievements so it must be loaded below Vanilla Achievements Expanded to avoid bugs.");
										}
										//else
										//{
										//    string curModVersion = modAssembly.GetName().Version.ToString();
										//    if (int.TryParse(string.Join("", curModVersion.Split('.')), out int modVersion) && modVersion < version)
										//    {
										//        Log.Warning($"[{mod.Name}] Using old version of AchievementsExpanded.dll, please inform the mod author to update to the newest version.");
										//    }
										//}
									}
								}
							}
						}
					}
				}
				catch(Exception ex)
				{
					Log.Error($"Exception thrown while checking Achievements load order. {ex.Message}");
				}
			}
		}
	}
}
