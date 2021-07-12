using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AchievementsExpanded
{
	[AttributeUsage(AttributeTargets.Method)]
	public class AchievementDebugAction : DebugActionAttribute
	{
		public bool requireBaseMod;

		public AchievementDebugAction(string category = null, string name = null, bool requireBaseMod = true, bool requiresRoyalty = false, bool requiresIdeology = false) : base(category, name, requiresRoyalty, requiresIdeology)
		{
			this.requireBaseMod = requireBaseMod;
		}
	}
}
