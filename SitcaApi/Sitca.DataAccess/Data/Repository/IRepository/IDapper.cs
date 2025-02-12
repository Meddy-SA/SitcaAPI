﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;

namespace Sitca.DataAccess.Data.Repository.IRepository
{
    public interface IDapper : IDisposable
    {
        DbConnection GetDbconnection();
        T Get<T>(
            string sp,
            DynamicParameters parms,
            CommandType commandType = CommandType.StoredProcedure
        );
        List<T> GetAll<T>(
            string sp,
            DynamicParameters parms,
            CommandType commandType = CommandType.StoredProcedure
        );
        Task<List<T>> GetAllAsync<T>(
            string sp,
            DynamicParameters parms,
            CommandType commandType = CommandType.StoredProcedure
        );
        int Execute(
            string sp,
            DynamicParameters parms,
            CommandType commandType = CommandType.StoredProcedure
        );
        T Insert<T>(
            string sp,
            DynamicParameters parms,
            CommandType commandType = CommandType.StoredProcedure
        );
        T Update<T>(
            string sp,
            DynamicParameters parms,
            CommandType commandType = CommandType.StoredProcedure
        );
    }
}
