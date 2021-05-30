using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
	public class MoodTracker : Tracker<float>
	{
		public MoodRating mood;
		public float average;

		public override string Key => "MoodTracker";

		public override MethodInfo MethodHook => AccessTools.Method(typeof(HistoryAutoRecorderWorker_ColonistMood), nameof(HistoryAutoRecorderWorker_ColonistMood.PullRecord));
		public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.AverageMoodColony));
		protected override string[] DebugText => new string[] { $"Requires mood to be: {mood}", $"Average: {average}" };

		public MoodTracker()
		{
		}

		public MoodTracker(MoodTracker reference) : base(reference)
		{
			mood = reference.mood;
			average = reference.average;

			if (average > 1 || average < 0)
				Log.Error("Unable to create MoodTracker. Average must be between 0 and 1");
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref mood, "mood");
			Scribe_Values.Look(ref average, "average");
		}

		public override bool Trigger(float avg)
		{
			base.Trigger(avg);
			if (mood == MoodRating.Below && avg <= (average * 100))
				return true;
			if (mood == MoodRating.Equal && avg == (average * 100))
				return true;
			if (mood == MoodRating.Above && avg >= (average * 100))
				return true;
			return false;
		}

		public enum MoodRating { Below, Equal, Above };
	}
}
