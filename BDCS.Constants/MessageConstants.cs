using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDCS.Constants
{
    public class MessageConstants
    {
        #region [Messages]
        public const string SuccessfullInsertion= "Record Added Successfully";
        public const string InsertionFailed = "Unable to Save Data";
        public const string SuccessfullDeletion = "Record Deleted Permanantly";
        public const string InternalError = "System Faced internal errorr";
        public const string NoDataReceived = "No Data Received";
        public const string UserNotAuthorized = "User Not Authorized";
        #endregion

        #region [MessageClasses]
        public const string MessageClassSuccess = "sucess";
        public const string MessageClassWarning = "warning";
        public const string MessageClassInfo = "info";
        public const string MessageClassError = "error";
        #endregion
    }
}
