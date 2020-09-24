using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
    public class BuildingGroupTracker : BuildingTracker
    {
        public override string Key => "BuildingGroupTracker";
        protected override string[] DebugText
        {
            get
            {
                string[] text = new string[0];
                foreach (var building in buildings)
                {
                    triggeredBuildingCount.TryGetValue(building.Key, out int current);
                    var entry = $"Kind: {building.Key?.defName ?? "None"} Count: {building.Value} Current: {current}";
                    text.AddItem(entry);
                }
                return text;
            }
        }

        public BuildingGroupTracker()
        {
        }

        public BuildingGroupTracker(BuildingGroupTracker reference) : base(reference)
        {
            buildings = reference.buildings;
            triggeredBuildingCount = new Dictionary<BuildableDef, int>();
            foreach (var build in buildings)
            {
                triggeredBuildingCount.Add(build.Key, 0);
            }
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref buildings, "buildings", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref triggeredBuildingCount, "triggeredBuildingCount", LookMode.Def, LookMode.Value);
        }

        public override bool Trigger(BuildableDef building, ThingDef stuff)
        {
            base.Trigger(building, stuff);
            if (buildings.ContainsKey(building))
            {
                triggeredBuildingCount[building]++;
                foreach (var build in buildings)
                {
                    if (triggeredBuildingCount[build.Key] < build.Value)
                        return false;
                }
                return true;
            }
            return false;
        }

        public Dictionary<BuildableDef, int> buildings = new Dictionary<BuildableDef, int>();

        public Dictionary<BuildableDef, int> triggeredBuildingCount = new Dictionary<BuildableDef, int>();
    }
}
