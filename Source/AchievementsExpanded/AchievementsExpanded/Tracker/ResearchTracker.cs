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
        public override string Key => "ResearchTracker";

        public override MethodInfo MethodHook => AccessTools.Method(typeof(ResearchManager), nameof(ResearchManager.FinishProject));
        public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.ResearchProjectFinished));
        protected override string[] DebugText => new string[] { $"Tech: {tech}", $"Project: {project?.defName ?? "None"}" };
        public ResearchTracker()
        {
        }

        public ResearchTracker(ResearchTracker reference) : base(reference)
        {
            project = reference.project;
            tech = reference.tech;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref project, "project");
            Scribe_Values.Look(ref tech, "tech");
        }

        public override bool Trigger(ResearchProjectDef proj)
        {
            if (project is null || project == proj)
            {
                if (tech != null)
                {
                    var researchProjs = (Dictionary<ResearchProjectDef, float>)AccessTools.Field(typeof(ResearchManager), "progress").GetValue(Current.Game.researchManager);
                    
                    foreach (var research in researchProjs.Where(r => r.Key.techLevel == tech.Value))
                    {
                        if (research.Value < research.Key.baseCost && research.Key.modContentPack.IsCoreMod)
                            return false;
                    }
                }
                return true;
            }
            return false;
        }

        public ResearchProjectDef project;
        public TechLevel? tech;
    }
}
