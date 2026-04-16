using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
//using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace IO.Swagger.Helpers
{
    public class NavWebServiceReference
    {
        public string Url;
        public string Domain;
        public string UserName;
        public string Password;
        public string ClientCredentialType;
    }
    
    public class Dal
    {
        private readonly string _connectionString;
        private readonly NavWebServiceReference _navWebServiceReference;

        public Dal(IConfiguration configuration, IOptions<NavWebServiceReferenceOptions> navWebServiceReferenceOptions)
        {
            _connectionString = configuration.GetSection("ConnectionStrings:NavConnectionString").Value;
            var opts = navWebServiceReferenceOptions.Value;
            _navWebServiceReference = new NavWebServiceReference
            {
                Url = opts.NavWebServiceReference,
                Domain = opts.Domain,
                UserName = opts.UserName,
                Password = opts.Password,
                ClientCredentialType = opts.ClientCredentialType ?? "Windows"
            };
        }

        #region Cs
        public string GetCs()
        {
            return _connectionString;
        }
        #endregion Cs

        #region WebReference
        public NavWebServiceReference GetNavWebReference()
        {
            return _navWebServiceReference;
        }
        #endregion WebReference


        private async Task SetConnectionOptionsAsync(SqlConnection connection)
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

        private void SetConnectionOptions(SqlConnection connection)
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

            cmd.ExecuteNonQuery();
        }

        #region +++++GetDataAsync
        //public async Task<DataSet> GetDataAsync(string spName, List<SqlParameter> spParam)
        //{
        //    using (SqlConnection con = new SqlConnection(GetCs()))
        //    {
        //        await con.OpenAsync();
        //        await SetConnectionOptionsAsync(con);
        //        SqlDataAdapter da = new SqlDataAdapter(spName, con);
        //        da.SelectCommand.CommandType = CommandType.StoredProcedure;
        //        foreach (SqlParameter par in spParam)
        //        {
        //            if (par.Value != null)
        //            {
        //                da.SelectCommand.Parameters.Add(par);
        //            }
        //        }
        //        DataSet ds = new DataSet();
        //        await Task.Run(() => da.Fill(ds));
        //        return ds;
        //    }
        //}

        public async Task<DataSet> GetDataAsync(string spName, List<SqlParameter> spParam)
        {
            var ds = new DataSet();
            using (var con = new SqlConnection(GetCs()))
            {
                await con.OpenAsync();
                await SetConnectionOptionsAsync(con);
                using (var cmd = new SqlCommand(spName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    foreach (var par in spParam)
                    {
                        if (par.Value != null)
                            cmd.Parameters.Add(par);
                    }
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        do
                        {
                            var dt = new DataTable();
                            dt.Load(reader); // Loads current result set
                            ds.Tables.Add(dt);
                        } while (!reader.IsClosed);
                    }
                }
            }
            return ds;
        }

        public async Task<DataSet> GetDataImprovedAsync(string spName, List<SqlParameter> spParam)
        {
            var ds = new DataSet();
            await using (var con = new SqlConnection(GetCs()))
            {
                await con.OpenAsync();
                await SetConnectionOptionsAsync(con);

                await using (var cmd = new SqlCommand(spName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (spParam != null)
                    {
                        foreach (var par in spParam)
                        {
                            if (par?.Value != null)
                                cmd.Parameters.Add(par);
                        }
                    }

                    await using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // Load first result set
                        var dt = new DataTable();
                        dt.Load(reader);
                        ds.Tables.Add(dt);

                        // Only loop if you expect multiple result sets
                        while (await reader.NextResultAsync())
                        {
                            dt = new DataTable();
                            dt.Load(reader);
                            ds.Tables.Add(dt);
                        }
                    }
                }
            }
            return ds;
        }

        public async Task<DataSet> GetDataAsync(string spName, SqlParameter spParam)
        {
            List<SqlParameter> parList = new List<SqlParameter>();
            parList.Add(spParam);

            return await GetDataAsync(spName, parList);
        }

        public async Task<DataSet> GetDataAsync(string spName)
        {
            return await GetDataAsync(spName, new SqlParameter(null, null));
        }
        #endregion

        #region GetData
        public DataSet GetData(string spName, List<SqlParameter> spParam)
        {
            using (SqlConnection con = new SqlConnection(GetCs()))
            {
                con.Open();
                SetConnectionOptions(con);
                SqlDataAdapter da = new SqlDataAdapter(spName, con);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                foreach (SqlParameter par in spParam)
                {
                    if (par.Value != null)
                    {
                        da.SelectCommand.Parameters.Add(par);
                    }
                }
                DataSet ds = new DataSet();
                da.Fill(ds);
                return ds;
            }
        }

        public DataSet GetData(string spName, SqlParameter spParam)
        {
            List<SqlParameter> parList = new List<SqlParameter>()
             {
                 spParam
             };
            return GetData(spName, parList);
        }

        public DataSet GetData(string spName)
        {
            return GetData(spName, new SqlParameter(null, null));
        }
        #endregion GetData

        #region GetDataReader
        public SqlDataReader GetDataReader(string spName, List<SqlParameter> spParam)
        {
            SqlConnection con = new SqlConnection(GetCs());
            con.Open();
            SetConnectionOptions(con);
            SqlCommand cmd = new SqlCommand(spName, con);
            cmd.CommandType = CommandType.StoredProcedure;
            foreach (SqlParameter par in spParam)
            {
                if (par.Value != null)
                {
                    cmd.Parameters.Add(par);
                }
            }
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }
        public SqlDataReader GetDataReader(string spName, SqlParameter spParam)
        {
            List<SqlParameter> parList = new List<SqlParameter>()
             {
                 spParam
             };
            return GetDataReader(spName, parList);
        }
        public SqlDataReader GetDataReader(string spName)
        {
            return GetDataReader(spName, new SqlParameter(null, null));
        }
        #endregion GetDataReader

        #region GetValue
        public string GetValue(string spName, List<SqlParameter> spParam)
        {
            using (SqlConnection con = new SqlConnection(GetCs()))
            {
                con.Open();
                SetConnectionOptions(con);
                SqlCommand cmd = new SqlCommand(spName, con);
                cmd.CommandType = CommandType.StoredProcedure;
                foreach (SqlParameter par in spParam)
                {
                    if (par.Value != null)
                    {
                        cmd.Parameters.Add(par);
                    }
                }
                return (string)cmd.ExecuteScalar();
            }
        }

        public string GetValue(string spName, SqlParameter spParam)
        {
            List<SqlParameter> parList = new List<SqlParameter>()
             {
                 spParam
             };
            return GetValue(spName, parList);
        }

        public string GetValue(string spName)
        {
            return GetValue(spName, new SqlParameter(null, null));
        }
        #endregion GetValue

        #region ++++++ExecSp
        public async Task<int> ExecSpAsync(string spName, List<SqlParameter> spParam)
        {
            using (SqlConnection con = new SqlConnection(GetCs()))
            {
                await con.OpenAsync();
                await SetConnectionOptionsAsync(con);
                SqlCommand cmd = new SqlCommand(spName, con);
                cmd.CommandType = CommandType.StoredProcedure;
                foreach (SqlParameter par in spParam)
                {
                    cmd.Parameters.Add(par);
                }
                return (int)await cmd.ExecuteScalarAsync();
            }
        }
        #endregion 

        #region ExecSp
        public int ExecSp(string spName, List<SqlParameter> spParam)
        {
            using (SqlConnection con = new SqlConnection(GetCs()))
            {
                con.Open();
                SetConnectionOptions(con);
                SqlCommand cmd = new SqlCommand(spName, con);
                cmd.CommandType = CommandType.StoredProcedure;
                foreach (SqlParameter par in spParam)
                {
                    cmd.Parameters.Add(par);
                }
                return (int)cmd.ExecuteScalar();
            }
        }

        public int ExecSp(string spName, SqlParameter spParam)
        {
            List<SqlParameter> parList = new List<SqlParameter>()
             {
                 spParam
             };
            return ExecSp(spName, parList);
        }

        public void ExecSp(string spName)
        {
            using (SqlConnection con = new SqlConnection(GetCs()))
            {
                con.Open();
                SetConnectionOptions(con);
                SqlCommand cmd = new SqlCommand(spName, con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
            }
        }

        public bool ExecSp(string spName, List<SqlParameter> spParam, out string _outParametar_metode)
        {
            bool _returnValue_metode = false;
            _outParametar_metode = string.Empty;
            using (SqlConnection con = new SqlConnection(GetCs()))
            {
                con.Open();
                SetConnectionOptions(con);
                SqlCommand cmd = new SqlCommand(spName, con);
                cmd.CommandType = CommandType.StoredProcedure;
                foreach (SqlParameter par in spParam)
                {
                    cmd.Parameters.Add(par);
                }
                _returnValue_metode = Convert.ToBoolean(cmd.ExecuteScalar());
                con.Close();
                foreach (SqlParameter _pa in cmd.Parameters)
                {
                    if (_pa.Direction == ParameterDirection.Output)
                    {
                        _outParametar_metode = _pa.Value.ToString();
                        break;
                    }
                }
            }
            return _returnValue_metode;
        }

        public int ExecSp2(string spName, List<SqlParameter> spParam, out string _outParametar_metode)
        {
            int _returnValue_metode;
            _outParametar_metode = string.Empty;
            using (SqlConnection con = new SqlConnection(GetCs()))
            {
                con.Open();
                SetConnectionOptions(con);
                SqlCommand cmd = new SqlCommand(spName, con);
                cmd.CommandType = CommandType.StoredProcedure;
                foreach (SqlParameter par in spParam)
                {
                    cmd.Parameters.Add(par);
                }
                _returnValue_metode = Convert.ToInt32(cmd.ExecuteScalar());
                con.Close();
                foreach (SqlParameter _pa in cmd.Parameters)
                {
                    if (_pa.Direction == ParameterDirection.Output)
                    {
                        _outParametar_metode = _pa.Value.ToString();
                        break;
                    }
                }
            }
            return _returnValue_metode;
        }
        #endregion ExecSp


    }
}
