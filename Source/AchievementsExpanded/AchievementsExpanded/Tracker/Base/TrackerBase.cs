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
		public int uniqueId = -1;
		///Reference to card this Tracker instance is currently assigned to
		public string cardAssigned; 

		/// <summary>
		/// Include only for XML instantiation
		/// </summary>
		public TrackerBase()
		{
		}

		/// <summary>
		/// Must include for Instance creation. 
		/// Passes in reference to Tracker of same type with fields populated from XML values
		/// </summary>
		/// <param name="reference"></param>
		public TrackerBase(TrackerBase reference)
		{
			uniqueId = Find.UniqueIDsManager.GetNextThingID();
		}

		/// <summary>
		/// Unique Key tracker used for HashSet retrieval from AchievementPointManager
		/// </summary>
		public abstract string Key { get; }

		/* ---------- Harmony Patch methods ---------- */
		/// Harmony patches are integrated as part of the Trackers for easier implementations
		/// <summary>
		/// Original method Harmony Patch will apply to
		/// </summary>
		public virtual MethodInfo MethodHook => null;
		/// <summary>
		/// Your method Harmony Patch will point to
		/// </summary>
		public virtual MethodInfo PatchMethod => null;
		/// <summary>
		/// Harmony patch type
		/// </summary>
		public virtual PatchType PatchType => PatchType.Postfix;
		/* ------------------------------------------- */

		/// <summary>
		/// Attach anonymous function to Long tick (~2000 ticks)
		/// </summary>
		/// <remarks>Useful when you need to check Trigger periodically rather than event based</remarks>
		public virtual Func<bool> AttachToLongTick => null;

		/// <summary>
		/// Checked on startup to determine if card should be unlocked
		/// </summary>
		public virtual bool UnlockOnStartup => false;

		/// <summary>
		/// Percentage complete of the current tracker. Only inherit if you want the completion bar to be displayed
		/// on the card in the UI window.
		/// </summary>
		public virtual (float percent, string text) PercentComplete => (-1f, null);

		/// <summary>
		/// Trigger event to check requirements if card should be unlocked or not
		/// </summary>
		public virtual bool Trigger()
		{
			return Trigger(string.Empty);
		}

		/// <summary>
		/// base most Trigger event
		/// ONLY FOR LOGGING DEBUG INFO
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public bool Trigger(string text = null)
		{
			DebugWriter.Log($"\nTrigger Event called for {GetUniqueLoadID()}");
			DebugWriter.Log($"Tracker Type: {GetType()}");
			if (!string.IsNullOrEmpty(text))
				DebugWriter.Log(text);
			DebugWriter.Log(DebugText);
			DebugWriter.Log($"Card: {cardAssigned}");
			return false;
		}

		/// <summary>
		/// Save method
		/// </summary>
		public virtual void ExposeData()
		{
			Scribe_Values.Look(ref uniqueId, "uniqueId", -1);
			Scribe_Values.Look(ref cardAssigned, "cardAssigned");
		}

		/// <summary>
		/// Array of debug info outputted to AchievementLog.txt file
		/// Best to include fields implemented by the tracker
		/// </summary>
		protected abstract string[] DebugText { get; }

		/// <summary>
		/// UniqueLoadID
		/// </summary>
		public virtual string GetUniqueLoadID() => $"{Key}_{uniqueId}";
	}
}
