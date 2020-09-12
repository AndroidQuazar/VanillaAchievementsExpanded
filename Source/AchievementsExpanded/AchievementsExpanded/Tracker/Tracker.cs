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
        public abstract bool Trigger(T param = default);
    }
}
