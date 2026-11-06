using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDCS.Data.Helpers
{
    public class SqlDbManager : IDBManger
    {
        SqlConnection _con;

        public SqlDbManager()
        {
            string ConString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            _con = new SqlConnection(ConString);
        }
        public int ExecuteNonQuery(string queryString, CommandType type, List<DbParameters> pars)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(queryString))
            {
                try
                {
                    OpenConnection();
                    SqlCommand com = _con.CreateCommand();
                    com.CommandText = queryString;
                    com.CommandType = type;
                    if (pars?.Count>0)
                    {
                        foreach (DbParameters pr in pars)
                        {
                            com.Parameters.AddWithValue(pr.Name, pr.Value == null ? DBNull.Value : pr.Value);
                        }
                    }
                    result=com.ExecuteNonQuery();
                }
                catch (Exception ex)
                {

                    throw;
                }
                finally
                {
                    CloseConnection();
                }
            }
            else
            {
                throw new ArgumentNullException("Query String", "Query String cannot be null");
            }
            return result;
        }

        #region ConnectionMethods
        private bool OpenConnection()
        {
            if (_con!=null && _con.State!=ConnectionState.Open)
            {
                try
                {
                    _con.Open();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return true;
        }
        private async Task<bool> OpenConnectionAsync()
        {
            if (_con!=null && _con.State!=ConnectionState.Open)
            {
                try
                {
                   await _con.OpenAsync();
                }
                catch (Exception e)
                {

                    throw;
                }
            }
            return true;
        }
        private void CloseConnection()
        {
            if (_con!=null && _con.State!=ConnectionState.Closed)
            {
                try
                {
                    _con.Close();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        #endregion

        public async Task<int> ExecuteNonQueryAsync(string queryString, CommandType type, List<DbParameters> pars)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(queryString))
            {
                try
                {
                    await OpenConnectionAsync();
                    SqlCommand com = _con.CreateCommand();
                    com.CommandText = queryString;
                    com.CommandType = type;
                    if (pars?.Count > 0)
                    {
                        foreach (DbParameters pr in pars)
                        {
                            com.Parameters.AddWithValue(pr.Name, pr.Value == null ? DBNull.Value : pr.Value);
                        }
                    }
                    result = await com.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {

                    throw;
                }
                finally
                {
                    CloseConnection();
                }
            }
            else
            {
                throw new ArgumentNullException("Query Sring", "Query String cannot be null");
            }
            return result; 
        }

        public string ExecuteScalar(string queryString, CommandType type, List<DbParameters> pars)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(queryString))
            {
                try
                {
                    OpenConnection();
                    SqlCommand com = _con.CreateCommand();
                    com.CommandText = queryString;
                    com.CommandType = type;
                    if (pars?.Count>0)
                    {
                        foreach (DbParameters pr in pars)
                        {
                            com.Parameters.AddWithValue(pr.Name, pr.Value == null ? DBNull.Value : pr.Value);
                        }
                    }
                    result = com.ExecuteScalar()?.ToString();
                }
                catch (Exception e)
                {

                    throw;
                }
                finally
                {
                    CloseConnection();
                }
            }
            else
            {
                throw new ArgumentNullException("Query String,Query String Cannot be null");
            }
            return result;
        }
        //scalar expect return value where non query direct eexcute query
        public async Task<string> ExecuteScalarAsync(string queryString, CommandType type, List<DbParameters> pars)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(queryString))
            {
                try
                {
                   await OpenConnectionAsync();
                    SqlCommand com = _con.CreateCommand();
                    com.CommandText = queryString;
                    com.CommandType = type;
                    if (pars?.Count > 0)
                    {
                        foreach (DbParameters pr in pars)
                        {
                            com.Parameters.AddWithValue(pr.Name, pr.Value == null ? DBNull.Value : pr.Value);
                        }
                    }
                    object obj = await com.ExecuteScalarAsync();
                    result = obj?.ToString();

                }

                catch (Exception e)
                {
                    throw;
                }
                finally
                {
                    CloseConnection();
                }
            }
            else
            {
                throw new ArgumentNullException("Query String,Query String Cannot be null");
            }
            return result;
        }

        public DataSet LoadDataSet(string queryString, CommandType type, List<DbParameters> pars)
        {
            DataSet ds = new DataSet();
            if (!string.IsNullOrEmpty(queryString))
            {
                try
                {
                    OpenConnection();
                    SqlCommand com = _con.CreateCommand();
                    com.CommandText = queryString;
                    com.CommandType = type;
                    com.CommandTimeout = 120;
                    if (pars?.Count>0)
                    {
                        foreach (DbParameters pr in pars)
                        {
                            com.Parameters.AddWithValue(pr.Name, pr.Value == null ? DBNull.Value : pr.Value);
                        }
                    }
                    SqlDataAdapter da = new SqlDataAdapter(com);
                    da.Fill(ds);
                    if (ds?.Tables.Count>0)
                    {

                    }
                }
                catch (Exception e)
                {
                    throw;
                }
                finally
                {
                    CloseConnection();
                }
            }
            else
            {
                throw new ArgumentNullException("Query String,Query String Cannot be null");
            }
            return ds;
        }

        public async Task<DataSet> LoadDataSetAsync(string queryString, CommandType type, List<DbParameters> pars)
      {
            DataSet ds = new DataSet();
            if (!string.IsNullOrEmpty(queryString))
            {
                try
                {
                    await OpenConnectionAsync();
                    SqlCommand com = _con.CreateCommand();
                    com.CommandText = queryString;
                    com.CommandType = type;
                    if (pars?.Count > 0)
                    {
                        foreach (DbParameters pr in pars)
                        {
                            com.Parameters.AddWithValue(pr.Name, pr.Value == null ? DBNull.Value : pr.Value);
                        }
                    }
                    SqlDataAdapter da = new SqlDataAdapter(com);
                    da.Fill(ds);
                    if (ds?.Tables.Count > 0)
                    {

                    }
                }
                catch (Exception )
                {

                    throw;
                }
                finally
                {
                    CloseConnection();
                } 
            }
            else
            {
                throw new ArgumentException("Query String,Query String Cannot be null");
            }
            return ds;
        }

        public DataTable LoadDataTable(string queryString, CommandType type, List<DbParameters> pars)
        {
            DataTable dt = new DataTable("Result");
            if (!string.IsNullOrEmpty(queryString))
            {
                try
                {
                    OpenConnection();
                    SqlCommand com = _con.CreateCommand();
                    com.CommandText = queryString;
                    com.CommandType = type;
                    if (pars?.Count>0)
                    {
                        foreach (DbParameters pr in pars)
                        {
                            //add parameters with values in the sqlcommand
                            com.Parameters.AddWithValue(pr.Name, pr.Value == null ? DBNull.Value : pr.Value);
                        }
                    }
                    SqlDataAdapter da = new SqlDataAdapter(com);
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    if (ds?.Tables.Count>0)
                    {
                        dt = ds.Tables[0];
                    }

                }
                catch (Exception e)
                {

                    throw;
                }
                finally
                {
                    CloseConnection();
                }
            }
            else
            {
                throw new ArgumentNullException("Query String,Query String Cannot be null");
            }
            return dt;
        }

        public async Task<DataTable> LoadDataTableAsync(string queryString, CommandType type, List<DbParameters> pars)
        {
            DataTable dt = new DataTable("Result");
            if (!string.IsNullOrEmpty(queryString))
            {
                try
                {
                    await OpenConnectionAsync();
                    SqlCommand com = _con.CreateCommand();
                    com.CommandText = queryString;
                    com.CommandType = type;
                    if (pars?.Count>0)
                    {
                        foreach (DbParameters pr in pars)
                        {
                            com.Parameters.AddWithValue(pr.Name, pr.Value == null ? DBNull.Value : pr.Value);
                        }
                    }
                    SqlDataAdapter da = new SqlDataAdapter(com);
                    DataSet ds = new DataSet();
                    if (ds?.Tables.Count>0)
                    {
                        dt = ds.Tables[0];
                    }
                }
                catch (Exception e)
                {

                    throw;
                }
                finally
                {
                    CloseConnection();
                }
            }
            else
            {
               throw new ArgumentNullException("Query String,Query String Cannot be null");
            }
            return dt;
        }
    }
}
