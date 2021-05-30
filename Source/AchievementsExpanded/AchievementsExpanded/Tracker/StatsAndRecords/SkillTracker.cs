using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
	public class SkillTracker : Tracker2<SkillDef, int>
	{
		public SkillDef def;
		public int level;
		public int count = 1;

		protected int triggeredCount;

		public override string Key => "SkillTracker";

		public override MethodInfo MethodHook => AccessTools.Method(typeof(SkillRecord), nameof(SkillRecord.Learn));
		public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.LevelUpMoteHook));
		public override PatchType PatchType => PatchType.Transpiler;
		protected override string[] DebugText => new string[] { $"Skill: {def?.defName ?? "None"}", $"Level: {level}", $"Count: {count}", $"Current: {triggeredCount}" };
		public SkillTracker()
		{
		}

		public SkillTracker(SkillTracker reference) : base(reference)
		{
			def = reference.def;
			level = reference.level;
			count = reference.count;
			triggeredCount = reference.triggeredCount;
		}

		public override (float percent, string text) PercentComplete => count > 1 ? ((float)triggeredCount / count, $"{triggeredCount} / {count}") : base.PercentComplete;

		public override bool UnlockOnStartup
		{
			get
			{
				foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction)
				{
					if (pawn.skills != null && Trigger(def, pawn.skills.GetSkill(def).Level))
					{
						return true;
					}
				}
				return false;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref level, "level");
			Scribe_Values.Look(ref count, "count", 1);
			Scribe_Values.Look(ref triggeredCount, "triggeredCount", 0);
		}

		

		public override bool Trigger(SkillDef skill, int level)
		{
			base.Trigger(skill, level);
			if ( (def is null || def == skill) && level >= this.level)
			{
				triggeredCount++;
			}
			return triggeredCount >= count;
		}
	}
}
