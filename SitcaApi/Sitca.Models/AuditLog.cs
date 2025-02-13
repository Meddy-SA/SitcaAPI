namespace Sitca.Models;

public class AuditLog
{
    public int Id { get; set; }
    public string TableName { get; set; } = null!;
    public string PrimaryKey { get; set; } = null!;
    public string ColumnName { get; set; } = null!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string Action { get; set; } = null!;
    public DateTime TimeStamp { get; set; }
    public string UserId { get; set; } = null!;
}
