namespace Sitca.Models.DTOs;

public class BlockResult<T>
{
    public List<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int BlockSize { get; set; }
    public int CurrentBlock { get; set; }
    public int TotalBlocks { get; set; }
    public bool HasMoreItems { get; set; }
    public int TotalPending { get; set; } = 0;
    public int TotalInProcess { get; set; } = 0;
    public int TotalCompleted { get; set; } = 0;
}
