using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
    public class SkillTracker : Tracker2<SkillDef, int>
    {
        public override string Key => "SkillTracker";

        public override MethodInfo MethodHook => AccessTools.Method(typeof(SkillRecord), nameof(SkillRecord.Learn));
        public override MethodInfo PatchMethod => AccessTools.Method(typeof(AchievementHarmony), nameof(AchievementHarmony.LevelUpMoteHook));
        public override PatchType PatchType => PatchType.Transpiler;
        protected override string[] DebugText => new string[] { $"Skill: {skill?.defName ?? "None"}", $"Level: {level}", $"Count: {count}", $"Current: {triggeredCount}" };
        public SkillTracker()
        {
        }

        public SkillTracker(SkillTracker reference) : base(reference)
        {
            skill = reference.skill;
            level = reference.level;
            count = reference.count;
            triggeredCount = reference.triggeredCount;

            logTracker = true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref skill, "skill");
            Scribe_Values.Look(ref level, "level");
            Scribe_Values.Look(ref count, "count", 1);
            Scribe_Values.Look(ref triggeredCount, "triggeredCount", 0);
        }

        public override bool Trigger(SkillDef skill, int level)
        {
            base.Trigger(skill, level);
            if ( (this.skill is null || this.skill == skill) && level >= this.level)
            {
                triggeredCount++;
            }
            return triggeredCount >= count;
        }

        public SkillDef skill;
        public int level;
        public int count = 1;

        public int triggeredCount;
    }
}
