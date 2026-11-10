using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BDCS.Data;
using BDCS.Model.RoleRights;

namespace BDCS.Bussiness
{
    public class LocationBL
    {
        readonly LocationDAL _locationDAL;

        public LocationBL()
        {
            _locationDAL = new LocationDAL();
        }

        public List<Location> GetLocationList()
            => _locationDAL.GetLocationList();

        public Location GetLocationById(int locationId)
            => _locationDAL.GetLocationById(locationId);

        public async Task<int> InsertUpdateLocation(Location location)
            => await _locationDAL.InsertUpdateLocation(location);
    }
}
