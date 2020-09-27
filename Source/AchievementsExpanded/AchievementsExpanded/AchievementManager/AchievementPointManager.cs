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
        internal Stack<AchievementCard> unlockedCards;

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
        }

        internal void HardReset()
        {
            achievementLookup = null;
            achievementList = null;
            unlockedCards = new Stack<AchievementCard>();
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
            if (unlockedCards is null)
                unlockedCards = new Stack<AchievementCard>();
            DebugWriter.ResetRootDir();
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

        public void DisplayCardWindow(AchievementCard card)
        {
            unlockedCards.Push(card);
        }

        public bool TryPurchasePoints(int points)
        {
            if (DebugSettings.godMode)
                return true;
            if (availablePoints < points)
                return false;
            availablePoints -= points;
            return true;
        }

        public override void GameComponentTick()
        {
            if (unlockedCards.Any() && !Find.WindowStack.Windows.Any(w => w.GetType() == typeof(AchievementNotification)))
            {
                Find.WindowStack.Add(new AchievementNotification(unlockedCards.Pop()));
                Log.Message($"DISPLAYING CARD! Count: {unlockedCards.Count}");
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref availablePoints, "points");
            Scribe_Values.Look(ref totalEarnedPoints, "totalEarnedPoints");

            Scribe_Collections.Look(ref achievementList, "achievementList", LookMode.Deep);
            Scribe_Collections.Look(ref unlockedCards, "unlockedCards", LookMode.Deep);
        }

        internal const string AchievementTag = "VanillaAchievementsExpanded";
    }
}
