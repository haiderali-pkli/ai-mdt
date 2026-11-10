using BDCS.Data.Helpers;
using BDCS.Model.RoleRights;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BDCS.Data
{
    public class LocationDAL
    {
        IDBManger _db;

        public LocationDAL()
        {
            _db = new SqlDbManager();
        }

        public List<Location> GetLocationList()
        {
            List<Location> Data = new List<Location>();
            string Query = QueryHelper.GetQueryString("GetLocationList", "System");
            List<DbParameters> pars = new List<DbParameters>();
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new Location
                    {
                        LocationId = Convert.ToInt32(a["LocationId"]),
                        LocationName = a["LocationName"].ToString(),
                        IsActive = a["IsActive"] != DBNull.Value && Convert.ToBoolean(a["IsActive"]),
                    }).ToList();
                }
            }
            catch (Exception)
            {

                throw;
            }
            return Data;
        }

        public Location GetLocationById(int locationId)
        {
            Location Data = new Location();
            string Query = QueryHelper.GetQueryString("GetLocationById", "System");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="LocationId", Type=DbType.Int32, Value=locationId }
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new Location
                    {
                        LocationId = Convert.ToInt32(a["LocationId"]),
                        LocationName = a["LocationName"].ToString(),
                        IsActive = a["IsActive"] != DBNull.Value && Convert.ToBoolean(a["IsActive"]),
                    }).ToList().FirstOrDefault();
                }
            }
            catch (Exception)
            {

                throw;
            }
            return Data;
        }

        public async Task<int> InsertUpdateLocation(Location location)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("InsertUpdateLocation", "System");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="LocationId", Type=DbType.Int32, Value=location.LocationId },
                new DbParameters() {Name="LocationName", Type=DbType.String, Value=location.LocationName },
                new DbParameters() {Name="IsActive", Type=DbType.Boolean, Value=location.IsActive },
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception)
            {

                throw;
            }
            return result;
        }
    }
}
