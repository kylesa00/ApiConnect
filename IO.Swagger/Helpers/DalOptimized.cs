using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace IO.Swagger.Helpers
{
    public class DalOptimized
    {
        private static string _cachedConnectionString;
        private static readonly object _lock = new object();

        #region Connection String (Cached)
        public static string GetCs()
        {
            if (_cachedConnectionString != null)
                return _cachedConnectionString;

            lock (_lock)
            {
                if (_cachedConnectionString != null)
                    return _cachedConnectionString;

                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true);
                var configuration = builder.Build();
                
                _cachedConnectionString = configuration.GetSection("ConnectionStrings").GetSection("NavConnectionString").Value;
                return _cachedConnectionString;
            }
        }
        #endregion

        #region GetDataReaderAsync
        public static async Task<SqlDataReader> GetDataReaderAsync(string spName, List<SqlParameter> spParam)
        {
            SqlConnection con = new SqlConnection(GetCs());
            
            var connectionTimer = System.Diagnostics.Stopwatch.StartNew();
            await con.OpenAsync();
            
            // *** CRITICAL FIX: Set ARITHABORT ON for all connections ***
            // This matches SSMS behavior and ensures consistent execution plans
            await SetConnectionOptionsAsync(con);
            
            connectionTimer.Stop();
            
            var executionTimer = System.Diagnostics.Stopwatch.StartNew();
            
            SqlCommand cmd = new SqlCommand(spName, con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;

            foreach (SqlParameter par in spParam)
            {
                if (par.Value != null)
                {
                    cmd.Parameters.Add(par);
                }
            }

            var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            executionTimer.Stop();
            
            Console.WriteLine($"[DAL] Connection Open: {connectionTimer.ElapsedMilliseconds}ms");
            Console.WriteLine($"[DAL] ExecuteReader: {executionTimer.ElapsedMilliseconds}ms");
            
            return reader;
        }

        /// <summary>
        /// Sets SQL Server connection options to match SSMS behavior.
        /// This ensures consistent execution plans and performance.
        /// </summary>
        private static async Task SetConnectionOptionsAsync(SqlConnection connection)
        {
            using var cmd = new SqlCommand(@"
                SET ARITHABORT ON;
                SET ANSI_NULLS ON;
                SET ANSI_PADDING ON;
                SET ANSI_WARNINGS ON;
                SET CONCAT_NULL_YIELDS_NULL ON;
                SET QUOTED_IDENTIFIER ON;
                SET NUMERIC_ROUNDABORT OFF;
            ", connection);
            
            await cmd.ExecuteNonQueryAsync();
        }

        public static async Task<SqlDataReader> GetDataReaderAsync(string spName, SqlParameter spParam)
        {
            List<SqlParameter> parList = new List<SqlParameter>() { spParam };
            return await GetDataReaderAsync(spName, parList);
        }

        public static async Task<SqlDataReader> GetDataReaderAsync(string spName)
        {
            return await GetDataReaderAsync(spName, new List<SqlParameter>());
        }
        #endregion

        #region GetDataAsync (For backward compatibility)
        public static async Task<DataSet> GetDataAsync(string spName, List<SqlParameter> spParam)
        {
            using (SqlConnection con = new SqlConnection(GetCs()))
            {
                await con.OpenAsync();
                
                // Set connection options
                await SetConnectionOptionsAsync(con);
                
                using (SqlCommand cmd = new SqlCommand(spName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;

                    foreach (SqlParameter par in spParam)
                    {
                        if (par.Value != null)
                        {
                            cmd.Parameters.Add(par);
                        }
                    }

                    DataSet ds = new DataSet();
                    
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        do
                        {
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            ds.Tables.Add(dt);
                        } while (!reader.IsClosed);
                    }
                    
                    return ds;
                }
            }
        }

        public static async Task<DataSet> GetDataAsync(string spName, SqlParameter spParam)
        {
            List<SqlParameter> parList = new List<SqlParameter>() { spParam };
            return await GetDataAsync(spName, parList);
        }

        public static async Task<DataSet> GetDataAsync(string spName)
        {
            return await GetDataAsync(spName, new List<SqlParameter>());
        }
        #endregion

        #region Helper Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetStringOrNull(SqlDataReader reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double? GetDoubleOrNull(SqlDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
                return null;

            var value = reader.GetValue(ordinal);
            
            if (value is decimal decimalValue)
                return (double)decimalValue;
            
            if (value is double doubleValue)
                return doubleValue;
            
            if (value is float floatValue)
                return (double)floatValue;
            
            return Convert.ToDouble(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool? GetBoolOrNull(SqlDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
                return null;

            var value = reader.GetValue(ordinal);
            
            if (value is bool boolValue)
                return boolValue;
            
            if (value is int intValue)
                return intValue != 0;
            
            if (value is byte byteValue)
                return byteValue != 0;
            
            if (value is short shortValue)
                return shortValue != 0;
            
            if (value is long longValue)
                return longValue != 0;
            
            return Convert.ToBoolean(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long? GetInt64OrNull(SqlDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
                return null;

            var value = reader.GetValue(ordinal);
            
            if (value is long longValue)
                return longValue;
            
            if (value is int intValue)
                return (long)intValue;
            
            if (value is decimal decimalValue)
                return (long)decimalValue;
            
            return Convert.ToInt64(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime? GetDateTimeOrNull(SqlDataReader reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? (DateTime?)null : reader.GetDateTime(ordinal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal? GetDecimalOrNull(SqlDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
                return null;

            var value = reader.GetValue(ordinal);
            
            if (value is decimal decimalValue)
                return decimalValue;
            
            if (value is double doubleValue)
                return (decimal)doubleValue;
            
            if (value is float floatValue)
                return (decimal)floatValue;
            
            return Convert.ToDecimal(value);
        }
        #endregion

        #region ExecSpAsync
        public static async Task<int> ExecSpAsync(string spName, List<SqlParameter> spParam)
        {
            using (SqlConnection con = new SqlConnection(GetCs()))
            {
                await con.OpenAsync();
                
                // Set connection options
                await SetConnectionOptionsAsync(con);
                
                SqlCommand cmd = new SqlCommand(spName, con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;

                foreach (SqlParameter par in spParam)
                {
                    cmd.Parameters.Add(par);
                }
              
                return (int) await cmd.ExecuteScalarAsync();
            }
        }
        #endregion
    }
}
