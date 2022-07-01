using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.IdentityServer.WebAPI.Settings
{
    public class LDAPSettings: ILDAPSettings
    {
        public string AD_domain { get; set; }
        public int AD_port { get; set; }
        public string ADMIN_USER { get; set; }
        public string ADMIN_PASS { get; set; }
        public string BASE_DN { get; set; }
        public string SEARCH_FILTER { get; set; }
    }
    public interface ILDAPSettings
    {
        public string AD_domain { get; set; }
        public int AD_port { get; set; }
        public string ADMIN_USER { get; set; }
        public string ADMIN_PASS { get; set; }
        public string BASE_DN { get; set; }
        public string SEARCH_FILTER { get; set; }
    }
}
