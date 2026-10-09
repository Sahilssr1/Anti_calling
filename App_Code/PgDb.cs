using System;
using System.Configuration;
using Npgsql;

/// <summary>
/// Local PostgreSQL access for the Sanjivani auto-call feature.
/// Database: sanjivani_calls (create with db/schema.sql).
/// Logs every call started from btnsipcall_Click and records whether
/// the Sanjivani audio message was played to the customer.
/// Requires the Npgsql NuGet package (v6.x for .NET Framework).
/// Connection string comes from Web.config -> connectionStrings -> "SanjivaniPg".
/// </summary>
public static class PgDb
{
    private static string ConnStr()
    {
        var cs = ConfigurationManager.ConnectionStrings["SanjivaniPg"];
        if (cs != null && !string.IsNullOrWhiteSpace(cs.ConnectionString))
            return cs.ConnectionString;

        // Fallback for local dev (matches db/schema.sql defaults).
        return "Host=localhost;Port=5432;Database=sanjivani_calls;Username=postgres;Password=postgres";
    }

    /// <summary>Current time in IST (Asia/Kolkata).</summary>
    public static DateTime IstNow()
    {
        try
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
        }
        catch
        {
            try
            {
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                    TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata"));
            }
            catch
            {
                return DateTime.UtcNow.AddHours(5.5);
            }
        }
    }

    /// <summary>morning (05-12) / afternoon (12-17) / evening (17-05) in IST.</summary>
    public static string GreetingForNow()
    {
        int h = IstNow().Hour;
        if (h >= 5 && h < 12) return "morning";
        if (h >= 12 && h < 17) return "afternoon";
        return "evening";
    }

    /// <summary>Insert a call_log row when the agent starts a call. Returns the new id.</summary>
    public static int LogCallStart(string mobileNo, string callerName, string greeting)
    {
        const string sql = "INSERT INTO call_log (mobile_no, caller_name, greeting_used) " +
                           "VALUES (@mobile, @caller, @greeting) RETURNING id;";
        using (var con = new NpgsqlConnection(ConnStr()))
        using (var cmd = new NpgsqlCommand(sql, con))
        {
            cmd.Parameters.AddWithValue("@mobile", (object)mobileNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@caller", (object)callerName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@greeting", (object)greeting ?? DBNull.Value);
            con.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }

    /// <summary>Mark the latest call to this number as "audio message played".</summary>
    public static void MarkAudioPlayed(string mobileNo, string callerName)
    {
        const string sql = "UPDATE call_log SET audio_played = TRUE, audio_played_at = NOW() " +
                           "WHERE id = (SELECT id FROM call_log WHERE mobile_no = @mobile " +
                           "ORDER BY id DESC LIMIT 1);";
        using (var con = new NpgsqlConnection(ConnStr()))
        using (var cmd = new NpgsqlCommand(sql, con))
        {
            cmd.Parameters.AddWithValue("@mobile", (object)mobileNo ?? DBNull.Value);
            con.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
