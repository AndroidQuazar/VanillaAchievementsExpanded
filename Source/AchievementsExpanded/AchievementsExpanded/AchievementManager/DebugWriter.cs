using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Verse;
using RimWorld;

namespace AchievementsExpanded
{
    public static class DebugWriter
    {
        public static bool Log(string text)
        {
            if (string.IsNullOrEmpty(RootDir))
                ResetRootDir();
            try
            {
                File.AppendAllLines(FullPath, new[] { text });
            }
            catch(Exception ex)
            {
                Verse.Log.Error($"Failed to Log {text} in Achievement DebugLog. Exception: {ex.Message}");
                return false;
            }
            return true;
        }

        public static bool Log(string[] text)
        {
            if (string.IsNullOrEmpty(RootDir))
                ResetRootDir();
            try
            {
                File.AppendAllLines(FullPath, text);
            }
            catch(Exception ex)
            {
                Verse.Log.Error($"Failed to Log {text} in Achievement DebugLog. Exception: {ex.Message}");
                return false;
            }
            return true;
        }

        internal static void ResetRootDir()
        {
            RootDir = ModLister.GetModWithIdentifier("smashphil.achievements").RootDir.ToString();
            Clear();
            Log(new string[] { "Vanilla Achievements Expanded", "This log is for logging Tracker information and Event triggers only.\n"});
        }

        public static void Clear()
        {
            File.WriteAllText(FullPath, string.Empty);
        }

        internal static string FullPath => $"{RootDir}\\{FileName}";
        internal static string RootDir;
        internal const string FileName = "AchievementLog.txt";
    }
}
