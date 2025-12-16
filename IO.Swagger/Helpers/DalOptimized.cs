using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace IO.Swagger.Helpers
{
    /// <summary>
    /// Optimized Data Access Layer with performance improvements:
    /// - Cached connection string
    /// - True async with SqlDataReader
    /// - Inlined helper methods
    /// - No Task.Run overhead
    /// </summary>
    public class DalOptimized
    {
        // Cache connection string to avoid rebuilding config every time
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

        #region GetDataReaderAsync (High Performance)
        /// <summary>
        /// High-performance async method that returns SqlDataReader for streaming data processing.
        /// IMPORTANT: Caller must dispose the reader. Connection will auto-close via CommandBehavior.CloseConnection
        /// </summary>
        public static async Task<SqlDataReader> GetDataReaderAsync(string spName, List<SqlParameter> spParam)
        {
            SqlConnection con = new SqlConnection(GetCs());
            SqlCommand cmd = new SqlCommand(spName, con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120; // 2 minutes timeout

            foreach (SqlParameter par in spParam)
            {
                if (par.Value != null)
                {
                    cmd.Parameters.Add(par);
                }
            }

            await con.OpenAsync();
            
            // CommandBehavior.CloseConnection ensures connection closes when reader is disposed
            return await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
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

        #region GetDataAsync (Optimized - For backward compatibility)
        /// <summary>
        /// Optimized async method using SqlDataReader instead of SqlDataAdapter.
        /// Still returns DataSet for backward compatibility with existing code.
        /// </summary>
        public static async Task<DataSet> GetDataAsync(string spName, List<SqlParameter> spParam)
        {
            using (SqlConnection con = new SqlConnection(GetCs()))
            {
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
                    await con.OpenAsync();
                    
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

        #region Helper Methods for Safe Reading (Inlined for Performance)
        /// <summary>
        /// Safely reads a string value from SqlDataReader, returning null if DBNull
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetStringOrNull(SqlDataReader reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        /// <summary>
        /// Safely reads a double value from SqlDataReader, returning null if DBNull.
        /// HANDLES BOTH DECIMAL AND DOUBLE from database!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double? GetDoubleOrNull(SqlDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
                return null;

            // FIX: Handle both decimal (common in SQL Server) and double
            var value = reader.GetValue(ordinal);
            
            if (value is decimal decimalValue)
                return (double)decimalValue;  // Convert decimal to double
            
            if (value is double doubleValue)
                return doubleValue;
            
            if (value is float floatValue)
                return (double)floatValue;
            
            // Fallback: try to convert whatever type it is
            return Convert.ToDouble(value);
        }

        /// <summary>
        /// Safely reads a bool value from SqlDataReader, returning null if DBNull.
        /// HANDLES BOTH BIT and INT (0/1) from database!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool? GetBoolOrNull(SqlDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
                return null;

            // FIX: Handle both BIT (boolean) and INT (0/1)
            var value = reader.GetValue(ordinal);
            
            if (value is bool boolValue)
                return boolValue;
            
            if (value is int intValue)
                return intValue != 0;  // 0 = false, anything else = true
            
            if (value is byte byteValue)
                return byteValue != 0;
            
            if (value is short shortValue)
                return shortValue != 0;
            
            if (value is long longValue)
                return longValue != 0;
            
            // Fallback: try to convert
            return Convert.ToBoolean(value);
        }

        /// <summary>
        /// Safely reads an Int64 value from SqlDataReader, returning null if DBNull
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long? GetInt64OrNull(SqlDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
                return null;

            // Handle different integer types
            var value = reader.GetValue(ordinal);
            
            if (value is long longValue)
                return longValue;
            
            if (value is int intValue)
                return (long)intValue;
            
            if (value is decimal decimalValue)
                return (long)decimalValue;
            
            return Convert.ToInt64(value);
        }

        /// <summary>
        /// Safely reads a DateTime value from SqlDataReader, returning null if DBNull
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime? GetDateTimeOrNull(SqlDataReader reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? (DateTime?)null : reader.GetDateTime(ordinal);
        }

        /// <summary>
        /// NEW: Safely reads a decimal value from SqlDataReader, returning null if DBNull
        /// </summary>
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

        #region ExecSpAsync (Optimized)
        public static async Task<int> ExecSpAsync(string spName, List<SqlParameter> spParam)
        {
            using (SqlConnection con = new SqlConnection(GetCs()))
            {
                SqlCommand cmd = new SqlCommand(spName, con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;

                foreach (SqlParameter par in spParam)
                {
                    cmd.Parameters.Add(par);
                }
                await con.OpenAsync();
              
                return (int) await cmd.ExecuteScalarAsync();
            }
        }
        #endregion
    }
}
