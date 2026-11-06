using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BDCS.Data;
using BDCS.Model.RoleRights;

namespace BDCS.Bussiness
{
    public class ClinicBL
    {
        readonly ClinicDAL _clinicDAL;

        public ClinicBL()
        {
            _clinicDAL = new ClinicDAL();
        }

        public List<Clinic> GetClinicList()
            => _clinicDAL.GetClinicList();

        public Clinic GetClinicById(int clinicId)
            => _clinicDAL.GetClinicById(clinicId);

        public async Task<int> InsertUpdateClinic(Clinic clinic)
            => await _clinicDAL.InsertUpdateClinic(clinic);
    }
}
