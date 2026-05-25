namespace NewsFlow.API;

/// <summary>
/// Shared cron-expression constants used when registering Hangfire recurring
/// jobs.  The actual <see cref="Hangfire.RecurringJob.AddOrUpdate"/> calls
/// live in <c>NewsFlow.Workers/Program.cs</c> where <c>IngestWorker</c> is
/// defined and the full DI service graph is available.
/// </summary>
public static class JobSchedules
{
    /// <summary>Every 5 minutes — RSS ingest + NewsAPI pull cycle.</summary>
    public const string IngestCycle = "*/5 * * * *";

    /// <summary>Daily at 02:00 UTC — analytics aggregation roll-up.</summary>
    public const string AnalyticsRollup = "0 2 * * *";

    /// <summary>Every hour — clean up expired OAuth PKCE state from cache.</summary>
    public const string OAuthCleanup = "0 * * * *";
}
