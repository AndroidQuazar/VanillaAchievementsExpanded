using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
	public class ResearchTracker : Tracker<ResearchProjectDef>
	{
		public ResearchProjectDef def;
		public TechLevel? tech;
		public bool coreModsOnly;

		public override string Key => "ResearchTracker";

		public override MethodInfo MethodHook => AccessTools.Method(typeof(ResearchManager), nameof(ResearchManager.FinishProject));
		public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.ResearchProjectFinished));
		protected override string[] DebugText => new string[] { $"Tech: {tech}", $"Project: {def?.defName ?? "None"}", $"CoreOnly: {coreModsOnly}" };
		public ResearchTracker()
		{
		}

		public ResearchTracker(ResearchTracker reference) : base(reference)
		{
			def = reference.def;
			tech = reference.tech;
			coreModsOnly = reference.coreModsOnly;
		}

		public override bool UnlockOnStartup
		{
			get
			{
				if (def is null && tech != null)
				{
					foreach (ResearchProjectDef research in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
					{
						if (research.techLevel == tech && Find.ResearchManager.GetProgress(research) < 1)
						{
							return false;
						}
					}
					return true;
				}
				else if (def != null && tech is null)
				{
					return Find.ResearchManager.GetProgress(def) >= 1;
				}
				return false;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref tech, "tech");
			Scribe_Values.Look(ref coreModsOnly, "coreModsOnly");
		}

		public override bool Trigger(ResearchProjectDef proj)
		{
			if (def is null || def == proj)
			{
				if (tech != null)
				{
					var researchProjs = (Dictionary<ResearchProjectDef, float>)AccessTools.Field(typeof(ResearchManager), "progress").GetValue(Current.Game.researchManager);
					
					foreach (var research in researchProjs.Where(r => r.Key.techLevel == tech))
					{
						if (research.Value < research.Key.baseCost && coreModsOnly && research.Key.modContentPack.IsCoreMod)
						{
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}
	}
}
