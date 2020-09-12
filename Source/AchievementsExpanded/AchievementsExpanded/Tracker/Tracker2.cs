using System;
using System.Collections.Generic;
using System.Linq;

namespace AchievementsExpanded
{
    public abstract class Tracker2<T1, T2> : TrackerBase
    {
        public Tracker2() { }
        public Tracker2(Tracker2<T1, T2> reference) : base(reference) { }
        public abstract bool Trigger(T1 param = default, T2 param2 = default);
    }
}
