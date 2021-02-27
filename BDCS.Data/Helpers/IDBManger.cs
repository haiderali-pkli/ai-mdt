using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDCS.Data.Helpers
{
    public interface IDBManger
    {
        int ExecuteNonQuery(string queryString, CommandType type, List<DbParameters> pars);
        Task<int> ExecuteNonQueryAsync(string queryString, CommandType type, List<DbParameters> pars);
        string ExecuteScalar(string queryString, CommandType type, List<DbParameters> pars);
        Task<string> ExecuteScalarAsync(string queryString, CommandType type, List<DbParameters> pars);
        DataTable LoadDataTable(string queryString, CommandType type, List<DbParameters> pars);
        Task<DataTable> LoadDataTableAsync(string queryString, CommandType type, List<DbParameters> pars);
        DataSet LoadDataSet(string queryString, CommandType type, List<DbParameters> pars);
        Task<DataSet> LoadDataSetAsync(string queryString, CommandType type, List<DbParameters> pars);
    }
}
