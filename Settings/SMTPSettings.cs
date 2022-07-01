using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.IdentityServer.WebAPI.Settings
{
    public class SMTPSettings : ISMTPSettings
    {
        public string SMTP_domain { get; set; }
        public int SMTP_port { get; set; }
    }
    public interface ISMTPSettings
    {
        public string SMTP_domain { get; set; }
        public int SMTP_port { get; set; }
    }
}
