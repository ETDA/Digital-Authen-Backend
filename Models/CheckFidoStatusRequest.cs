﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.IdentityServer.WebAPI.Models
{
    public class CheckFidoStatusRequest
    {
        [Required]
        public string username { get; set; }
    }
}
