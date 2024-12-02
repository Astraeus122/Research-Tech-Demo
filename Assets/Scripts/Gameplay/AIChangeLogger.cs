using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Unity.BossRoom.Gameplay.AI
{
    public static class AIChangeLogger
    {
        private static List<LogEntry> logEntries = new List<LogEntry>();
        private static string logFilePath = Application.dataPath + "/Scripts/Logs/AIChangesLog.json"; // Updated to match Scripts/Logs

        /// <summary>
        /// Logs changes to stats, including what triggered the change.
        /// </summary>
        /// <param name="statName">The name of the stat being adjusted.</param>
        /// <param name="oldValue">The previous value of the stat.</param>
        /// <param name="newValue">The new value of the stat after adjustment.</param>
        /// <param name="reason">The general reason for the change (e.g., "Rule-Based Adjustment").</param>
        /// <param name="condition">Specific trigger for the change (e.g., "Too many deaths").</param>
        public static void LogChange(string statName, float oldValue, float newValue, string reason, string condition)
        {
            LogEntry entry = new LogEntry
            {
                Timestamp = DateTime.UtcNow.ToString("o"), // ISO 8601 format
                StatName = statName,
                OldValue = oldValue,
                NewValue = newValue,
                Reason = reason,
                Condition = condition
            };

            logEntries.Add(entry);

            Debug.Log($"Logged Change: {entry}");
        }

        /// <summary>
        /// Saves all logged changes to a JSON file.
        /// </summary>
        public static void SaveLog()
        {
            // Ensure the directory exists
            string directoryPath = Path.GetDirectoryName(logFilePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Debug.Log($"Directory created: {directoryPath}");
            }

            // Convert log entries to JSON and write to file
            try
            {
                string json = JsonUtility.ToJson(new LogData { Entries = logEntries }, prettyPrint: true);
                File.WriteAllText(logFilePath, json);
                Debug.Log($"AI Change Log saved to {logFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save log file: {ex.Message}");
            }
        }

        /// <summary>
        /// Clears all logged entries (useful for testing or starting a new session).
        /// </summary>
        public static void ClearLog()
        {
            logEntries.Clear();
            Debug.Log("AI Change Log cleared.");
        }

        /// <summary>
        /// Represents a single log entry for AI changes.
        /// </summary>
        [Serializable]
        private class LogEntry
        {
            public string Timestamp; // When the change was logged
            public string StatName;  // Name of the stat being adjusted
            public float OldValue;   // Value before the change
            public float NewValue;   // Value after the change
            public string Reason;    // General reason (e.g., "RL Adjustment")
            public string Condition; // Specific trigger (e.g., "Too many deaths")

            public override string ToString()
            {
                return $"[{Timestamp}] {StatName}: {OldValue} -> {NewValue}, Reason: {Reason}, Condition: {Condition}";
            }
        }

        /// <summary>
        /// Wrapper class for serializing the log entries to JSON.
        /// </summary>
        [Serializable]
        private class LogData
        {
            public List<LogEntry> Entries;
        }
    }
}
