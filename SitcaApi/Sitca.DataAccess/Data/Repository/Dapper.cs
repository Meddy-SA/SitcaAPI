using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sitca.DataAccess.Data.Repository.IRepository;

namespace Sitca.DataAccess.Data.Repository
{
    public class Dapperr : IDapper
    {
        private readonly IConfiguration _config;
        private string Connectionstring = "DefaultConnection";
        private readonly ILogger<Dapperr> _logger;

        public Dapperr(IConfiguration config, ILogger<Dapperr> logger)
        {
            _config = config;
            _logger = logger;
        }

        public void Dispose() { }

        public int Execute(
            string sp,
            DynamicParameters parms,
            CommandType commandType = CommandType.StoredProcedure
        )
        {
            throw new NotImplementedException();
        }

        public T Get<T>(
            string sp,
            DynamicParameters parms,
            CommandType commandType = CommandType.Text
        )
        {
            using IDbConnection db = new SqlConnection(
                _config.GetConnectionString(Connectionstring)
            );
            return db.Query<T>(sp, parms, commandType: commandType).FirstOrDefault();
        }

        public List<T> GetAll<T>(
            string sp,
            DynamicParameters parms,
            CommandType commandType = CommandType.StoredProcedure
        )
        {
            using IDbConnection db = new SqlConnection(
                _config.GetConnectionString(Connectionstring)
            );
            return db.Query<T>(sp, parms, commandType: commandType).ToList();
        }

        /// <summary>
        /// Ejecuta un stored procedure o query y retorna una lista de resultados
        /// </summary>
        /// <typeparam name="T">Tipo de entidad a retornar</typeparam>
        /// <param name="sp">Nombre del stored procedure o query</param>
        /// <param name="parms">Parámetros del SP</param>
        /// <param name="commandType">Tipo de comando (SP o Text)</param>
        /// <returns>Lista de resultados</returns>
        /// <exception cref="DatabaseException">Error al ejecutar el comando</exception>
        public async Task<List<T>> GetAllAsync<T>(
            string sp,
            DynamicParameters parms,
            CommandType commandType = CommandType.StoredProcedure
        )
        {
            try
            {
                using IDbConnection db = new SqlConnection(
                    _config.GetConnectionString(Connectionstring)
                );

                var query = await db.QueryAsync<T>(sp, parms, commandType: commandType);
                return query.ToList();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error ejecutando SP: {SP}. Error: {Message}", sp, ex.Message);
                throw new Exception($"Error ejecutando SP: {sp}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado ejecutando SP: {SP}", sp);
                throw;
            }
        }

        public DbConnection GetDbconnection()
        {
            return new SqlConnection(_config.GetConnectionString(Connectionstring));
        }

        public T Insert<T>(
            string sp,
            DynamicParameters parms,
            CommandType commandType = CommandType.StoredProcedure
        )
        {
            T result;
            using IDbConnection db = new SqlConnection(
                _config.GetConnectionString(Connectionstring)
            );
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                using var tran = db.BeginTransaction();
                try
                {
                    result = db.Query<T>(sp, parms, commandType: commandType, transaction: tran)
                        .FirstOrDefault();
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                    db.Close();
            }

            return result;
        }

        public T Update<T>(
            string sp,
            DynamicParameters parms,
            CommandType commandType = CommandType.StoredProcedure
        )
        {
            T result;
            using IDbConnection db = new SqlConnection(
                _config.GetConnectionString(Connectionstring)
            );
            try
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                using var tran = db.BeginTransaction();
                try
                {
                    result = db.Query<T>(sp, parms, commandType: commandType, transaction: tran)
                        .FirstOrDefault();
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                    db.Close();
            }

            return result;
        }
    }
}
