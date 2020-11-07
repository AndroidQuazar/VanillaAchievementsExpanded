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
            MessageLogsLimitReached(1);
            try
            {
                //File.AppendAllLines(FullPath, new[] { text });
                messageLogs.Add(text);
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
            MessageLogsLimitReached(text.Length);
            try
            {
                //File.AppendAllLines(FullPath, text);
                messageLogs.AddRange(text);
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
            RootDir = ModLister.GetModWithIdentifier(AchievementHarmony.modIdentifier).RootDir.ToString();
            Clear();
            messageLogs.Clear();
            Log(new string[] { "Vanilla Achievements Expanded", "This log is for logging Tracker information and Event triggers only.\n"});
        }

        public static void Clear()
        {
            File.WriteAllText(FullPath, string.Empty);
            if (messageLogs is null)
                messageLogs = new List<string>();
        }

        private static void MessageLogsLimitReached(int adding)
        {
            if (messageLogs.Count + adding > MaxMessageLimit)
            {
                for (int i = 0; i < adding; i++)
                {
                    messageLogs.RemoveAt(0);
                }

                if (messageLogs.Count > MaxMessageLimit)
                {
                    messageLogs.Clear();
                    Verse.Log.Error("WENT OVER LOG LIMIT");
                }
            }
        }

        public static void PushToFile()
        {
            Clear();
            string successMessage = "SuccessfulWriteToFile".Translate();
            MessageTypeDef messageType = MessageTypeDefOf.TaskCompletion;
            try
            {
                File.AppendAllLines(FullPath, messageLogs);
            }
            catch(Exception ex)
            {
                successMessage = $"Failed to push logs to file. Exception: {ex.Message}";
                messageType = MessageTypeDefOf.RejectInput;
            }
            Messages.Message(successMessage, messageType);
        }

        internal static List<string> messageLogs = new List<string>();
        private const int MaxMessageLimit = 3000;

        internal static string FullPath => $"{RootDir}\\{FileName}";
        internal static string RootDir;
        internal const string FileName = "AchievementLog.txt";
    }
}
