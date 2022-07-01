using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.IdentityServer.WebAPI.Models
{
    public class ADUser
    {
        //public string EmployeeId { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string Department { get; set; }
        public string Title { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
    }
}
