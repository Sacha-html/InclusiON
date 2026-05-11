namespace InclusiON.Infrastructure.Configuration;

public class BackgroundJobSettings
{
    public WorkerSettings Worker { get; set; } = new();
    public RetryPolicySettings RetryPolicies { get; set; } = new();
    public CircuitBreakerSettings CircuitBreaker { get; set; } = new();
    public PythonAgentSettings PythonAgent { get; set; } = new();

    public class WorkerSettings
    {
        public int PendingJobsIntervalSeconds { get; set; } = 60;
        public int MidnightCleanupHour { get; set; } = 3;
        public int BatchSize { get; set; } = 10;
        public int OrphanTimeoutMinutes { get; set; } = 5;
    }

    public class RetryPolicySettings
    {
        public int EmbeddingMaxRetries { get; set; } = 3;
        public int EmbeddingBaseDelaySeconds { get; set; } = 1;
        public int EmailMaxRetries { get; set; } = 5;
        public int EmailBaseDelaySeconds { get; set; } = 2;
        public int DefaultMaxRetries { get; set; } = 3;
    }

    public class CircuitBreakerSettings
    {
        public double FailureThreshold { get; set; } = 0.5;
        public int MinThroughput { get; set; } = 10;
        public int BreakDurationSeconds { get; set; } = 30;
    }

    public class PythonAgentSettings
    {
        public string Url { get; set; } = "http://localhost:5001";
        public int TimeoutSeconds { get; set; } = 60;
    }
}
