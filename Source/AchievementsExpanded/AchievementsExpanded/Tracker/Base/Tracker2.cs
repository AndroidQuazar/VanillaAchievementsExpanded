using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AchievementsExpanded
{
	public abstract class Tracker2<T1, T2> : TrackerBase
	{
		public Tracker2() { }
		public Tracker2(Tracker2<T1, T2> reference) : base(reference) { }
		public virtual bool Trigger(T1 param = default, T2 param2 = default) => base.Trigger(string.Concat($"Param 1: { param.ToStringSafe() }", $" Param 2: { param2.ToStringSafe() }"));
	}
}
