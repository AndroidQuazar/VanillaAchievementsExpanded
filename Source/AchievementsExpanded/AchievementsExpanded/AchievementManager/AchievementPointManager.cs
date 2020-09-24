using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace AchievementsExpanded
{
    public class AchievementPointManager : GameComponent
    {

        public int availablePoints;
        public int totalEarnedPoints;

        public static Dictionary<TrackerBase, HashSet<AchievementCard>> achievementLookup = new Dictionary<TrackerBase, HashSet<AchievementCard>>();

        internal HashSet<AchievementCard> achievementList;

        internal static HashSet<AchievementCard> AchievementList => Current.Game?.GetComponent<AchievementPointManager>()?.achievementList ?? new HashSet<AchievementCard>();
        internal static IEnumerable<Type> TrackerTypes { get; set; }
        internal static List<TrackerBase> TrackersGenerated { get; set; }

        public AchievementPointManager(Game game)
        {
        }

        public static void OnStartUp()
        {
            TrackerTypes = GenTypes.AllTypes.Where(t => t.IsSubclassOf(typeof(TrackerBase)) && !t.IsAbstract);
            TrackersGenerated = new List<TrackerBase>();
            foreach (Type t in TrackerTypes)
            {
                var tracker = (TrackerBase)Activator.CreateInstance(t, null);
                TrackersGenerated.Add(tracker);
            }
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            PreInit();
        }

        public override void LoadedGame()
        {
            base.FinalizeInit();
            PreInit();
            DebugWriter.RootDir = ModLister.GetModWithIdentifier("smashphil.achievements").RootDir.ToString();
            DebugWriter.Clear();
            DebugWriter.Log(new string[] { "Vanilla Achievements Expanded", "This log is for logging Tracker information and Event triggers only.\n"});
        }

        internal void HardReset()
        {
            achievementLookup = null;
            achievementList = null;
            DebugWriter.Log($"Regenerating achievements...\n");
            PreInit(true);
            ResetPoints();
        }

        private void PreInit(bool debug = false)
        {
            if (achievementLookup is null)
                achievementLookup = new Dictionary<TrackerBase, HashSet<AchievementCard>>();
            if (achievementList is null)
                achievementList = new HashSet<AchievementCard>();
            if(debug)
                DebugWriter.Log($"Resetting AchievementLinks");
            achievementLookup = AchievementGenerator.GenerateAchievementLinks(achievementList);
            if(debug)
                DebugWriter.Log($"Verifying Achievement List");
            AchievementGenerator.VerifyAchievementList(ref achievementList, debug);
        }

        public void ResetAchievement(AchievementCard card)
        {
            achievementList.Remove(card);
            achievementList.Add(new AchievementCard(card.def));
        }

        public void ResetPoints()
        {
            availablePoints = 0;
            totalEarnedPoints = 0;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref availablePoints, "points");
            Scribe_Values.Look(ref totalEarnedPoints, "totalEarnedPoints");

            Scribe_Collections.Look(ref achievementList, "achievementList", LookMode.Deep);
        }

        internal const string AchievementTag = "VanillaAchievementsExpanded";
    }
}
