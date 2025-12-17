namespace Sitca.Models.DTOs.Dashboard
{
    public class SystemStatusDto
    {
        public ApiStatusDto ApiStatus { get; set; } = new();
        public DatabaseStatusDto DatabaseStatus { get; set; } = new();
        public ActivityMetricsDto ActivityMetrics { get; set; } = new();
        public ServerHealthDto ServerHealth { get; set; } = new();
    }

    public class ApiStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public string ResponseTime { get; set; } = string.Empty;
        public DateTime LastCheck { get; set; }
    }

    public class DatabaseStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public int ConnectionCount { get; set; }
        public DateTime LastCheck { get; set; }
    }

    public class ActivityMetricsDto
    {
        public int ActiveUsersToday { get; set; }
        public DateTime? LastSystemActivity { get; set; }
        public string LastActivityType { get; set; } = string.Empty;
        public int NotificationsSentToday { get; set; }
        public int CertificationsStartedThisWeek { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ServerHealthDto
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
    }
}