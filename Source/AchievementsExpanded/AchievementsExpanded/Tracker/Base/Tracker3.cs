using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AchievementsExpanded
{
	public abstract class Tracker3<T1, T2, T3> : TrackerBase
	{
		public Tracker3() { }
		public Tracker3(Tracker3<T1, T2, T3> reference) : base(reference) { }
		public virtual bool Trigger(T1 param = default, T2 param2 = default, T3 param3 = default) => base.Trigger(string.Concat($"Param 1: { param.ToStringSafe() }", $" Param 2: { param2.ToStringSafe() }", $" Param 3: { param3.ToStringSafe() }"));
	}
}
