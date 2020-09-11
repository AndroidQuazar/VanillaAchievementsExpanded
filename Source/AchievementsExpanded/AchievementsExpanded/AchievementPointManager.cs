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

        public List<AchievementCard> activeAchievements = new List<AchievementCard>();

        public AchievementPointManager(Game game)
        {
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();

            AchievementGenerator.GenerateAchievementsInit();
        }

        public override void LoadedGame()
        {
            base.FinalizeInit();
            AchievementGenerator.VerifyAchievementList(ref activeAchievements);
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

            Scribe_Collections.Look(ref activeAchievements, "activeAchievements");
        }
    }
}
