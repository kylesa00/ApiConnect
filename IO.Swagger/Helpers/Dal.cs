using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
//using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient;
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
    }
    public class Dal
    {
        #region Cs
        //public static string cs = @"Server=NAVSRV-BG-01\SQL2012;Initial Catalog=WagenInternational_NAV_Connect_Integration;User ID=sqlHellper4ApiConnect;Password=sqlH#llp#r4@p!C0nn#ct";

        public static string GetCs()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true);
            var configuation = builder.Build();
            return configuation.GetSection("ConnectionStrings").GetSection("NavConnectionString").Value;
        }
        #endregion Cs

        #region WebReference
        public static NavWebServiceReference GetNavWebReference()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var configuation = builder.Build();

            return new NavWebServiceReference()
            {
                Url = configuation.GetSection("WebServiceReference").GetSection("NavWebServiceReference").Value,
                Domain = configuation.GetSection("WebServiceReference").GetSection("Domain").Value,
                UserName = configuation.GetSection("WebServiceReference").GetSection("UserName").Value,
                Password = configuation.GetSection("WebServiceReference").GetSection("Password").Value,
            };
        }

        #endregion WebReference

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
    
        /// <summary>
        /// Sets SQL Server connection options to match SSMS behavior (synchronous version).
        /// This ensures consistent execution plans and performance.
        /// </summary>      
        private static void SetConnectionOptions(SqlConnection connection)
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
        public static async Task<DataSet> GetDataAsync(string spName, List<SqlParameter> spParam)
        {
            using (SqlConnection con = new SqlConnection(GetCs()))
            {
                await con.OpenAsync();
                
                // Set connection options
                await SetConnectionOptionsAsync(con);
                
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
                await Task.Run(() => da.Fill(ds));
                return ds;
            }
        }

        public static async Task<DataSet> GetDataAsync(string spName, SqlParameter spParam)
        {
            List<SqlParameter> parList = new List<SqlParameter>()
             {
                 spParam
             };

            return await GetDataAsync(spName, parList);
        }

        public static async Task<DataSet> GetDataAsync(string spName)
        {
            return await GetDataAsync(spName, new SqlParameter(null, null));
        }
        #endregion

        #region GetData
        public static DataSet GetData(string spName, List<SqlParameter> spParam)
        {
            using (SqlConnection con = new SqlConnection(GetCs()))
            {
                con.Open();
                
                // Set connection options
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

        public static DataSet GetData(string spName, SqlParameter spParam)
        {
            List<SqlParameter> parList = new List<SqlParameter>()
             {
                 spParam
             };

            return GetData(spName, parList);
        }

        public static DataSet GetData(string spName)
        {
            return GetData(spName, new SqlParameter(null, null));
        }

        #endregion GetData

        #region GetDataReader
        //metoda za izvlacenje podataka iz baze sa parametrom, vraca datareader
        //(za razliku od predhodne da mozes dodeliti odredjenu vrednost odredjenoj kontroli)
        public static SqlDataReader GetDataReader(string spName, List<SqlParameter> spParam)
        {
            SqlConnection con = new SqlConnection(GetCs());
            con.Open();
            
            // Set connection options
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

            //ovako se poziva i koristi

            //SqlDataReader dr = adoMetode.GetDataReader("p_dajPrijavu_zaIdP", new SqlParameter("@IdP", e.CommandArgument.ToString()));
            //while (dr.Read()){
            //    lblOpisP.Text = dr["NaslovP"].ToString();

            //}
            //dr.Close();
        }
        public static SqlDataReader GetDataReader(string spName, SqlParameter spParam)
        {
            List<SqlParameter> parList = new List<SqlParameter>()
             {
                 spParam
             };

            return GetDataReader(spName, parList);
        }
        //isto samo bez parametara
        public static SqlDataReader GetDataReader(string spName)
        {
            return GetDataReader(spName, new SqlParameter(null, null));
        }

        #endregion GetDataReader

        #region GetValue
        public static string GetValue(string spName, List<SqlParameter> spParam)
        {
            using (SqlConnection con = new SqlConnection(GetCs()))
            {
                con.Open();
                
                // Set connection options
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

        public static string GetValue(string spName, SqlParameter spParam)
        {
            List<SqlParameter> parList = new List<SqlParameter>()
             {
                 spParam
             };

            return GetValue(spName, parList);
        }

        public static string GetValue(string spName)
        {
            return GetValue(spName, new SqlParameter(null, null));
        }
        #endregion GetValue

        #region ++++++ExecSp
        //metoda za izvrsavanje stored procedura za listu parametara
        public static async Task<int> ExecSpAsync(string spName, List<SqlParameter> spParam)
        {
            using (SqlConnection con = new SqlConnection(GetCs()))
            {
                await con.OpenAsync();
                
                // Set connection options
                await SetConnectionOptionsAsync(con);
                
                SqlCommand cmd = new SqlCommand(spName, con);
                cmd.CommandType = CommandType.StoredProcedure;

                foreach (SqlParameter par in spParam)
                {
                    cmd.Parameters.Add(par);
                }
              
                return (int) await cmd.ExecuteScalarAsync();
            }
        }
        #endregion 

        #region ExecSp
        //metoda za izvrsavanje stored procedura za listu parametara
        public static int ExecSp(string spName, List<SqlParameter> spParam)
        {
            using (SqlConnection con = new SqlConnection(GetCs()))
            {
                con.Open();
                
                // Set connection options
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

        //metoda za izvrsavanje stored procedura za parametar
        public static int ExecSp(string spName, SqlParameter spParam)
        {
            List<SqlParameter> parList = new List<SqlParameter>()
             {
                 spParam
             };
            return ExecSp(spName, parList);
        }

        //metoda za izvrsavanje stored procedura bez parametra
        public static void ExecSp(string spName)
        {
            using (SqlConnection con = new SqlConnection(GetCs()))
            {
                con.Open();
                
                // Set connection options
                SetConnectionOptions(con);
                
                SqlCommand cmd = new SqlCommand(spName, con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.ExecuteNonQuery();
            }
        }

        //metoda za izvrsavanje stored procedura za listu parametara sa output parametrom
        public static bool ExecSp(string spName, List<SqlParameter> spParam, out string _outParametar_metode)
        {
            bool _returnValue_metode = false;
            _outParametar_metode = string.Empty;
            using (SqlConnection con = new SqlConnection(GetCs()))
            {
                con.Open();
                
                // Set connection options
                SetConnectionOptions(con);

                SqlCommand cmd = new SqlCommand(spName, con);
                cmd.CommandType = CommandType.StoredProcedure;

                foreach (SqlParameter par in spParam)
                {
                    cmd.Parameters.Add(par);
                }

                _returnValue_metode = Convert.ToBoolean(cmd.ExecuteScalar());
                con.Close();

                foreach (SqlParameter _pa in cmd.Parameters)    //nadji u listi parametara, sql Output parametar, i dodeli ga out parametru metode
                {
                    if (_pa.Direction == ParameterDirection.Output)
                    {
                        _outParametar_metode = _pa.Value.ToString();
                        break;
                    }
                }
            }

            return _returnValue_metode;
            //ovako se poziva i koristi
            //protected void daj()
            //{
            //    List<SqlParameter> param = new List<SqlParameter>()
            //        {
            //            new SqlParameter(){
            //                ParameterName="@IdP",
            //                Value = Label1.Text
            //            },
            //           new SqlParameter() {
            //                ParameterName = "@spOutParam",
            //                Direction = ParameterDirection.Output,
            //                SqlDbType = SqlDbType.VarChar,
            //                Size = 20
            //                }
            //};

            //------------------------------------------------------------------------------
            //    string aaa = string.Empty;
            //    adoMetode.ExecSp("p_r_test1", param, out aaa);

            //    Response.Write(aaa);
            //}

            //------------------------------------------------------------------------------------------------------------------------------
        }

        public static int ExecSp2(string spName, List<SqlParameter> spParam, out string _outParametar_metode)
        {
            int _returnValue_metode;
            _outParametar_metode = string.Empty;
            using (SqlConnection con = new SqlConnection(GetCs()))
            {
                con.Open();
                
                // Set connection options
                SetConnectionOptions(con);

                SqlCommand cmd = new SqlCommand(spName, con);
                cmd.CommandType = CommandType.StoredProcedure;

                foreach (SqlParameter par in spParam)
                {
                    cmd.Parameters.Add(par);
                }

                _returnValue_metode = Convert.ToInt32(cmd.ExecuteScalar());
                con.Close();

                foreach (SqlParameter _pa in cmd.Parameters)    //nadji u listi parametara, sql Output parametar, i dodeli ga out parametru metode
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
