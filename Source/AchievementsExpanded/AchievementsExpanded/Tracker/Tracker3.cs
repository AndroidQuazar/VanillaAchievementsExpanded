using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AchievementsExpanded
{
    public abstract class Tracker3<T1, T2, T3> : TrackerBase
    {
        public Tracker3() { }
        public Tracker3(Tracker3<T1, T2, T3> reference) : base(reference) { }
        public abstract bool Trigger(T1 param = default, T2 param2 = default, T3 param3 = default);
    }
}
