using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace WAPP.Utils
{
    // 1. Add Enum to prevent typos across your app
    public enum LogLevel
    {
        DEBUG,
        INFO,
        NOTICE,
        WARNING,
        ERROR,
        CRITICAL,
        EMERGENCY
    }

    public static class SystemLogService
    {
        private static readonly string ConnStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

        // 2. Cache dictionary to prevent N+1 DB queries
        private static readonly Dictionary<string, int> _severityCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _cacheLock = new object();

        // 3. Updated signature using the Enum and removing the default "OPEN" status
        public static void Write(string actionType, string description, LogLevel severity, int? userId = null, string explicitStatus = null)
        {
            if (!string.IsNullOrEmpty(description) && description.Length > 255)
            {
                description = description.Substring(0, 250) + "...";
            }
            try
            {
                string severityName = severity.ToString();
                int severityId = GetCachedSeverityId(severityName);

                // Smart status assignment: WARNING (or higher) is OPEN. INFO/NOTICE/DEBUG is RESOLVED.
                string finalStatus = explicitStatus ?? (IsAlertLevel(severity) ? "OPEN" : "RESOLVED");

                using (SqlConnection con = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(@"INSERT INTO systemLog (user_id, severity_id, action_type, description, status, created_at)
                                                       VALUES (@uid, @sev, @act, @desc, @stat, @created)", con))
                {
                    cmd.Parameters.AddWithValue("@uid", (object)userId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@sev", severityId);
                    cmd.Parameters.AddWithValue("@act", actionType);
                    cmd.Parameters.AddWithValue("@desc", description);
                    cmd.Parameters.AddWithValue("@stat", finalStatus);
                    cmd.Parameters.AddWithValue("@created", DateTime.Now); // Consider DateTime.UtcNow if servers cross timezones

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // TEMPORARY DEBUGGING: Throw the error so it breaks the screen and tells us what's wrong!
                throw new Exception("DATABASE LOGGING FAILED: " + ex.Message);
            }
        }

        private static bool IsAlertLevel(LogLevel level)
        {
            // WARNING, ERROR, CRITICAL, EMERGENCY trigger an "OPEN" status
            return level >= LogLevel.WARNING;
        }

        private static int GetCachedSeverityId(string severityName)
        {
            // Check cache first to save a DB trip
            lock (_cacheLock)
            {
                if (_severityCache.ContainsKey(severityName))
                {
                    return _severityCache[severityName];
                }
            }

            // If not in cache, fetch from DB
            int fetchedId = 1; // Fallback ID
            using (SqlConnection con = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 id FROM logSeverity WHERE name = @name", con))
            {
                cmd.Parameters.AddWithValue("@name", severityName);
                con.Open();
                object result = cmd.ExecuteScalar();
                if (result != null && int.TryParse(result.ToString(), out int id))
                {
                    fetchedId = id;
                }
            }

            // Save to cache for next time
            lock (_cacheLock)
            {
                _severityCache[severityName] = fetchedId;
            }

            return fetchedId;
        }
    }
}