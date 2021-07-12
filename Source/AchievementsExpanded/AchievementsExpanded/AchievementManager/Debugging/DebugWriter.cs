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
		private const int MaxMessageLimit = 3000;
		internal const string FileName = "AchievementLog.txt";

		internal static List<string> messageLogs = new List<string>();
		
		internal static string RootDir;
		internal static bool writerDisabled = false;

		internal static string FullPath => $"{RootDir}\\{FileName}";

		public static bool Log(string text)
		{
			if (!writerDisabled && Prefs.DevMode)
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
			}
			return true;
		}

		public static bool Log(string[] text)
		{
			if (!writerDisabled && Prefs.DevMode)
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
			}
			return true;
		}

		internal static void ResetRootDir()
		{
			RootDir = ModLister.GetModWithIdentifier(AchievementHarmony.modIdentifier)?.RootDir.ToString();
			if (string.IsNullOrEmpty(RootDir))
			{
				Verse.Log.Message($"{AchievementPointManager.AchievementTag} Disabling DebugWriter. Vanilla Achievements Expanded mod not found in mod list.");
				writerDisabled = true;
				return;
			}
			Clear();
			messageLogs.Clear();
			Log(new string[] { "Vanilla Achievements Expanded", "This log is for logging Tracker information and Event triggers only.\n"});
		}

		public static void Clear()
		{
			if (!writerDisabled)
			{
				File.WriteAllText(FullPath, string.Empty);
				if (messageLogs is null)
					messageLogs = new List<string>();
			}
		}

		private static void MessageLogsLimitReached(int adding)
		{
			while (messageLogs.Count + adding > MaxMessageLimit)
			{
				messageLogs.RemoveAt(0);
			}
		}

		public static void PushToFile()
		{
			if (writerDisabled)
			{
				Verse.Log.Warning($"{AchievementPointManager.AchievementTag} Cannot push to file. DebugWriter is disabled. Vanilla Achievements Expanded must be downloaded and in the mod list in order to use this feature.");
				Verse.Log.TryOpenLogWindow();
				return;
			}
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
	}
}
