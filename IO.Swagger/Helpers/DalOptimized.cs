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

        #region GetDataReaderAsync - WITH AD-HOC BATCH OPTIMIZATION
        /// <summary>
        /// High-performance async method using ad-hoc batch execution (like SSMS)
        /// instead of RPC calls. This avoids parameter sniffing and XML Reader overhead.
        /// </summary>
        public static async Task<SqlDataReader> GetDataReaderAsync(string spName, List<SqlParameter> spParam)
        {
            SqlConnection con = new SqlConnection(GetCs());
            
            // Check if we're calling GetAvailabilities with TVP - use optimized path
            bool isAvailabilitiesWithTVP = spName == "GetAvailabilities" && 
                spParam.Any(p => p.TypeName == "dbo.tyAvailabilityRequest");

            if (isAvailabilitiesWithTVP)
            {
                // OPTIMIZATION: Use batch execution instead of RPC
                return await GetDataReaderAsync_AdHocBatch(con, spName, spParam);
            }
            else
            {
                // Standard RPC call for other procedures
                return await GetDataReaderAsync_RPC(con, spName, spParam);
            }
        }

        /// <summary>
        /// Standard RPC execution (original method)
        /// </summary>
        private static async Task<SqlDataReader> GetDataReaderAsync_RPC(
            SqlConnection con, 
            string spName, 
            List<SqlParameter> spParam)
        {
            var connectionTimer = System.Diagnostics.Stopwatch.StartNew();
            await con.OpenAsync();
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
            
            // Log the timings (you'll see these in console)
            Console.WriteLine($"[DAL] Connection Open: {connectionTimer.ElapsedMilliseconds}ms");
            Console.WriteLine($"[DAL] ExecuteReader: {executionTimer.ElapsedMilliseconds}ms");
            
            return reader;
        }

        /// <summary>
        /// Ad-hoc batch execution (SSMS-style) - MUCH FASTER!
        /// Avoids RPC overhead and parameter sniffing issues
        /// </summary>
        private static async Task<SqlDataReader> GetDataReaderAsync_AdHocBatch(
            SqlConnection con,
            string spName,
            List<SqlParameter> spParam)
        {
            // Build the T-SQL batch manually (like SSMS does)
            var sql = new System.Text.StringBuilder();
            
            // Declare table variable
            sql.AppendLine("DECLARE @availabilityRequest dbo.tyAvailabilityRequest;");
            
            // Find the TVP parameter
            var tvpParam = spParam.FirstOrDefault(p => p.TypeName == "dbo.tyAvailabilityRequest");
            var companyParam = spParam.FirstOrDefault(p => p.ParameterName == "@company");
            
            if (tvpParam?.Value is DataTable dt)
            {
                // Insert data into table variable
                foreach (DataRow row in dt.Rows)
                {
                    sql.AppendLine($@"INSERT INTO @availabilityRequest VALUES (
                        '{row["articleId"].ToString().Replace("'", "''")}',
                        {row["quantity"]},
                        '{row["customerNr"].ToString().Replace("'", "''")}',
                        '{row["sendMethod"]?.ToString().Replace("'", "''") ?? ""}',
                        {((bool)row["partialDelivery"] ? "1" : "0")},
                        '{row["deliveryAddressId"]?.ToString().Replace("'", "''") ?? ""}',
                        '{row["pickupBranchId"]?.ToString().Replace("'", "''") ?? ""}',
                        '{row["pickingWarehouse"]?.ToString().Replace("'", "''") ?? ""}',
                        {((bool)row["isTourTimetable"] ? "1" : "0")}
                    );");
                }
            }
            
            // Call the stored procedure with OPTION (RECOMPILE) to get fresh plan
            sql.AppendLine($@"
EXEC {spName} 
    @company = '{companyParam?.Value.ToString().Replace("'", "''")}',
    @availabilityRequest = @availabilityRequest
OPTION (RECOMPILE);");

            // Execute as ad-hoc batch (like SSMS)
            SqlCommand cmd = new SqlCommand(sql.ToString(), con);
            cmd.CommandType = CommandType.Text;  // ← KEY DIFFERENCE: Text, not StoredProcedure
            cmd.CommandTimeout = 120;

            await con.OpenAsync();
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

        #region Helper Methods (unchanged)
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
    }
}
