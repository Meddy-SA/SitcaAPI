using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sitca.Models.DTOs;

namespace Sitca.DataAccess.Services.ProcessQuery;

public static class QueryableExtensions
{
    public static async Task<BlockResult<T>> ToBlockResultAsync<T>(
        this IQueryable<T> source,
        int blockNumber,
        int blockSize
    )
    {
        if (blockNumber < 1)
            blockNumber = 1;
        if (blockSize < 1)
            blockSize = 100;

        var totalCount = await source.CountAsync();
        var totalBlocks = (int)Math.Ceiling(totalCount / (double)blockSize);

        var items = await source.Skip((blockNumber - 1) * blockSize).Take(blockSize).ToListAsync();

        return new BlockResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            BlockSize = blockSize,
            CurrentBlock = blockNumber,
            TotalBlocks = totalBlocks,
            HasMoreItems = blockNumber < totalBlocks,
        };
    }
}
