using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.IdentityServer.WebAPI.Settings
{
    public class FidoSettings : IFidoSettings
    {
        public string CheckStatus_URL { get; set; }
        public string SetBasicAuth_URL { get; set; }
        public string Credentials { get; set; }
        public string API_Key { get; set; }
    }

    public interface IFidoSettings
    {
        public string CheckStatus_URL { get; set; }
        public string SetBasicAuth_URL { get; set; }
        public string Credentials { get; set; }
        public string API_Key { get; set; }
    }
}
