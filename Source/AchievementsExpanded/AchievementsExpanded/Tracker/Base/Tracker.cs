using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace AchievementsExpanded
{
	public abstract class Tracker<T> : TrackerBase
	{
		public Tracker() { }
		public Tracker(Tracker<T> reference) : base(reference) { }
		public virtual bool Trigger(T param = default) => base.Trigger($"Param 1: { param.ToStringSafe() }");
	}
}
