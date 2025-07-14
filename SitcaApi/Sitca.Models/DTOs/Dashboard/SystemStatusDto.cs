namespace Sitca.Models.DTOs.Dashboard
{
    public class SystemStatusDto
    {
        public ApiStatusDto ApiStatus { get; set; } = new();
        public DatabaseStatusDto DatabaseStatus { get; set; } = new();
        public LastBackupDto LastBackup { get; set; } = new();
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

    public class LastBackupDto
    {
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
    }

    public class ServerHealthDto
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
    }
}