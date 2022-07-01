using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.IdentityServer.WebAPI.Models
{
    public class EmailRequest
    {
        [Required]
        public string from_address { get; set; }

        [Required]
        public string to_address { get; set; }

        [Required]
        public string subject { get; set; }

        [Required]
        public string activation_code { get; set; }
    }
}
