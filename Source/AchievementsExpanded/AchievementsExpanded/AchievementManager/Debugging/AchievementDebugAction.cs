using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AchievementsExpanded
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AchievementDebugActionAttribute : DebugActionAttribute
    {
		public AchievementDebugActionAttribute(string category = null, string name = null, bool requireBaseMod = true) : base(category, name)
		{
			this.requireBaseMod = requireBaseMod;
		}

		public bool requireBaseMod;
    }
}
