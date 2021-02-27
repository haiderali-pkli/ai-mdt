using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDCS.Model.Helper
{
    public class AjaxResponse
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public string MessageTitle { get; set; }
        public string MessageClass { get; set; } = "info";
        public string RedirectUrl { get; set; }
        public bool IsSuccess { get; set; }
        public bool DoFullRedirect { get; set; }
        public object ResponseObject { get; set; }
        public string PopupUrl { get; set; }

    }
}
