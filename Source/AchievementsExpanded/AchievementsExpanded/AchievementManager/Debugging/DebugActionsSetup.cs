using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Verse;
using HarmonyLib;

namespace AchievementsExpanded
{
	public static class DebugActionsSetup
	{
		private static HashSet<MethodInfo> actionsAdded = new HashSet<MethodInfo>();

		public static void ClearCachedActions()
		{
			actionsAdded = new HashSet<MethodInfo>();
		}

		public static bool GenerateCacheForVAEDebugActions(MethodInfo method, DebugActionAttribute attribute)
		{
			bool vaeActive = UtilityMethods.BaseModActive;
			if (method.TryGetAttribute(out AchievementDebugAction vaeAttribute))
			{
				return (vaeActive || !vaeAttribute.requireBaseMod) && actionsAdded.Add(method);
			}
			return true;
		}
	}
}
