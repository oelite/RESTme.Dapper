﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace OElite.Restme.Dapper
{
    public partial class RestmeDb
    {
        public async Task<IList<T>> FetchEnumerableAsync<T>(string query, object paramValues,
            bool paginatedQuery = false,
            CommandType? dbCommandType = null, int commandTimeout = 0)
        {
            try
            {
                var results =
                    await (await GetOpenConnectionAsync()).QueryAsync<T>(query, paramValues, _currentTransaction,
                        commandType: dbCommandType, commandTimeout: commandTimeout);
                var enumerable = results as IList<T> ?? results.ToList();
                return enumerable;
            }
            catch (Exception ex)
            {
                Logger?.LogError($"Fetching from db failed\n {ex.Message},ex");
                throw ex;
            }
        }

        public async Task<T> FetchAsync<T>(string standardQuery, object paramValues, CommandType? dbCommandType = null,
            int commandTimeout = 0)
            where T : class
        {
            try
            {
                Logger?.LogInformation($"Fetching using DB query: \n {standardQuery} ");
                Logger?.LogInformation($"DB query parameters: \n {paramValues?.JsonSerialize()}");
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var result = await (await GetOpenConnectionAsync()).QueryFirstOrDefaultAsync<T>(standardQuery,
                    paramValues,
                    _currentTransaction, commandType: dbCommandType, commandTimeout: commandTimeout);
                if (stopwatch.ElapsedMilliseconds >= ExecutionPerformanceThresholdInMs)
                {
                    Logger?.LogWarning($"DB query execution time: \n {stopwatch.ElapsedMilliseconds} ms");
                }
                else
                {
                    Logger?.LogInformation($"DB query execution time: \n {stopwatch.ElapsedMilliseconds} ms");
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger?.LogError($"Fetching from db failed\n {ex.Message}", ex);

                throw ex;
            }
        }

        public async Task<TC> FetchAsync<T, TC>(string query, object paramValues, bool paginatedQuery = false,
            CommandType? dbCommandType = null, int commandTimeout = 0)
            where TC : IRestmeDbEntityCollection<T>, new() where T : IRestmeDbEntity
        {
            try
            {
                SqlMapper.AddTypeMap(typeof(long), DbType.Int32);
                Logger?.LogInformation($"Fetching using DB query: \n {query} ");
                Logger?.LogInformation($"DB query parameters: \n {paramValues?.JsonSerialize()}");
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var resultSet = new TC();
                // if (paginatedQuery)
                // {
                //     var results =
                //         await (await GetOpenConnectionAsync()).QueryMultipleAsync(query, paramValues,
                //             _currentTransaction,
                //             commandType: dbCommandType);
                //     var totalCount = await results.ReadSingleOrDefaultAsync<int>();
                //     var result = results.Read<T>().AsList();
                //     if (totalCount <= 0) return resultSet;
                //
                //     resultSet.TotalRecordsCount = Convert.ToInt32(totalCount);
                //     if (result.Any())
                //         resultSet.AddRange(result);
                // }
                // else
                // {
                var results =
                    await (await GetOpenConnectionAsync()).QueryAsync<T>(query, paramValues, _currentTransaction,
                        commandType: dbCommandType, commandTimeout: commandTimeout);
                var enumerable = results?.ToList();
                resultSet.TotalRecordsCount = enumerable.Count;

                if (paginatedQuery && enumerable?.Count > 0)
                {
                    var firstItem = enumerable.FirstOrDefault();
                    if (firstItem.BaseSearchCount > 0 && firstItem.BaseSearchCount >= resultSet.TotalRecordsCount)
                        resultSet.TotalRecordsCount = firstItem.BaseSearchCount;
                }

                if (enumerable.Any())
                    resultSet.AddRange(enumerable);
                // }

                if (stopwatch.ElapsedMilliseconds >= ExecutionPerformanceThresholdInMs)
                {
                    Logger?.LogWarning($"DB query execution time: \n {stopwatch.ElapsedMilliseconds} ms");
                }
                else
                {
                    Logger?.LogInformation($"DB query execution time: \n {stopwatch.ElapsedMilliseconds} ms");
                }

                return resultSet;
            }
            catch (Exception ex)
            {
                Logger?.LogError(
                    $"Fetching from db failed\n {ex.Message}\n dbConnection: {_dbConnectionString}\n query: {query}",
                    ex);
                throw ex;
            }
        }

        public async Task<long> ExecuteInsertAsync(string standardQuery, object paramValues,
            CommandType? dbCommandType = null, int commandTimeout = 0)
        {
            try
            {
                Logger?.LogInformation($"Executing insert DB query: \n {standardQuery} ");
                Logger?.LogInformation($"DB query parameters: \n {paramValues?.JsonSerialize()}");
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var result =
                    await
                        (await GetOpenConnectionAsync()).QuerySingleOrDefaultAsync<long>(standardQuery, paramValues,
                            _currentTransaction, commandType: dbCommandType, commandTimeout: commandTimeout);

                Logger?.LogInformation($"DB query results: \n {result}");
                if (stopwatch.ElapsedMilliseconds >= ExecutionPerformanceThresholdInMs)
                {
                    Logger?.LogWarning($"DB query execution time: \n {stopwatch.ElapsedMilliseconds} ms");
                }
                else
                {
                    Logger?.LogInformation($"DB query execution time: \n {stopwatch.ElapsedMilliseconds} ms");
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger?.LogError($"Executing DB query failed: \n {ex.Message}", ex);
                throw ex;
            }
        }

        public async Task<T> ExecuteInsertAsync<T>(string standardQuery, object paramValues,
            CommandType? dbCommandType = null, int commandTimeout = 0)
        {
            try
            {
                Logger?.LogInformation($"Executing insert DB query: \n {standardQuery} ");
                Logger?.LogInformation($"DB query parameters: \n {paramValues?.JsonSerialize()}");
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var result =
                    await
                        (await GetOpenConnectionAsync()).QuerySingleOrDefaultAsync<T>(standardQuery, paramValues,
                            _currentTransaction, commandType: dbCommandType, commandTimeout: commandTimeout);

                Logger?.LogInformation($"DB query results: \n {result}");
                if (stopwatch.ElapsedMilliseconds >= ExecutionPerformanceThresholdInMs)
                {
                    Logger?.LogWarning($"DB query execution time: \n {stopwatch.ElapsedMilliseconds} ms");
                }
                else
                {
                    Logger?.LogInformation($"DB query execution time: \n {stopwatch.ElapsedMilliseconds} ms");
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger?.LogError($"Executing DB query failed: \n {ex.Message}", ex);
                throw ex;
            }
        }

        public async Task<int> ExecuteAsync(string standardQuery, object paramValues, CommandType? dbCommandType = null,
            int commandTimeout = 0)
        {
            try
            {
                Logger?.LogInformation($"Executing DB query: \n {standardQuery} ");
                Logger?.LogInformation($"DB query parameters: \n {paramValues?.JsonSerialize()}");
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var result =
                    await
                        (await GetOpenConnectionAsync()).ExecuteAsync(standardQuery, paramValues, _currentTransaction,
                            commandType: dbCommandType, commandTimeout: commandTimeout);

                Logger?.LogInformation($"DB query results: \n {result}");
                if (stopwatch.ElapsedMilliseconds >= ExecutionPerformanceThresholdInMs)
                {
                    Logger?.LogWarning($"DB query execution time: \n {stopwatch.ElapsedMilliseconds} ms");
                }
                else
                {
                    Logger?.LogInformation($"DB query execution time: \n {stopwatch.ElapsedMilliseconds} ms");
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger?.LogError($"Executing DB query failed: \n {ex.Message}", ex);
                throw ex;
            }
        }

        public async Task<T> ExecuteScalarAsync<T>(string standardQuery, object paramValues,
            CommandType? dbCommandType = null, int commandTimeout = 0)
        {
            try
            {
                Logger?.LogInformation($"Executing DB query: \n {standardQuery} ");
                Logger?.LogInformation($"DB query parameters: \n {paramValues?.JsonSerialize()}");
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var result = await (await GetOpenConnectionAsync()).ExecuteScalarAsync<T>(standardQuery, paramValues,
                    _currentTransaction,
                    commandType: dbCommandType, commandTimeout: commandTimeout);
                Logger?.LogInformation($"DB query results: \n {result}");
                if (stopwatch.ElapsedMilliseconds >= ExecutionPerformanceThresholdInMs)
                {
                    Logger?.LogWarning($"DB query execution time: \n {stopwatch.ElapsedMilliseconds} ms");
                }
                else
                {
                    Logger?.LogInformation($"DB query execution time: \n {stopwatch.ElapsedMilliseconds} ms");
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger?.LogError($"Executing DB query failed: \n {ex.Message}", ex);
                throw ex;
            }
        }
    }
}