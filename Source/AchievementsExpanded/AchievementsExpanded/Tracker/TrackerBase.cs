using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace AchievementsExpanded
{
    public enum PatchType { Prefix, Transpiler, Postfix, Finalizer };
    public abstract class TrackerBase : IExposable, ILoadReferenceable
    {
        public TrackerBase()
        {
        }

        public TrackerBase(TrackerBase reference)
        {
            uniqueId = Find.UniqueIDsManager.GetNextThingID();
        }

        public abstract string Key { get; }

        public virtual MethodInfo MethodHook => null;
        public virtual MethodInfo PatchMethod => null;
        public virtual Func<bool> AttachToLongTick => null;

        public virtual PatchType PatchType => PatchType.Postfix;

        public virtual bool Trigger()
        {
            return Trigger(string.Empty);
        }

        public bool Trigger(string text = null)
        {
            DebugWriter.Log($"\nTrigger Event called for {GetUniqueLoadID()}");
            if (!string.IsNullOrEmpty(text))
                DebugWriter.Log(text);
            DebugWriter.Log(DebugText);
            DebugWriter.Log($"Card: {cardAssigned}");
            return false;
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref uniqueId, "uniqueId", -1);
            Scribe_Values.Look(ref logTracker, "logTracker", false);
            Scribe_Values.Look(ref cardAssigned, "cardAssigned");
        }

        protected abstract string[] DebugText { get; }

        public virtual string GetUniqueLoadID() => $"{Key}_{uniqueId}";

        public int uniqueId = -1;
        public bool logTracker;
        public string cardAssigned;
    }
}
