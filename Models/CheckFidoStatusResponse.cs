using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.IdentityServer.WebAPI.Models
{
    public class CheckFidoStatusResponse
    {
        public string status { get; set; }
        public string description { get; set; }
        public string allow_basic_auth { get; set; }
        public int allow_expire_unix { get; set; }
    }
}
