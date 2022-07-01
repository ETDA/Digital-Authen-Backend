using Authentication.IdentityServer.WebAPI.Models;
using Authentication.IdentityServer.WebAPI.Services;
using Authentication.IdentityServer.WebAPI.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Authentication.IdentityServer.WebAPI.Controllers
{
    [ApiController]
    [Route("idp/v1")]
    public class SuperAdminController : Controller
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IFidoSettings _fidoConfig;
        private readonly AdminService _adminService;
        public SuperAdminController(ILoggerFactory loggerFactory, AdminService adminService, IFidoSettings fidoConfig)
        {
            loggerFactory =
               LoggerFactory.Create(builder =>
                   builder.AddSimpleConsole(options =>
                   {
                       options.IncludeScopes = true;
                       options.SingleLine = true;
                       options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss]: ";
                   }));
            _loggerFactory = loggerFactory;
            _adminService = adminService;
            _fidoConfig = fidoConfig;
        }

        [HttpPost("CheckSuperAdmin")]
        public ActionResult checkSuperAdmin([FromHeader] string apikey, [FromBody] SuperAdminRequest body)
        {
            var _logger = _loggerFactory.CreateLogger<SuperAdminController>();

            SuperAdminResponse result = new SuperAdminResponse();

            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("---------- Start CheckSuperAdmin Session() ----------");

                    if (apikey != _fidoConfig.API_Key)
                    {
                        result.result = "error";
                        result.description = "apikey Invalid";
                        _logger.LogError(result.description);
                        return Unauthorized(result);
                    }

                    _logger.LogInformation("Username: " + body.username);

                    SHA512 sha512Hash = SHA512.Create();

                    //From String to byte array
                    byte[] sourceBytes = Encoding.UTF8.GetBytes(body.password);
                    byte[] hashBytes = sha512Hash.ComputeHash(sourceBytes);
                    string hash_passwd = BitConverter.ToString(hashBytes).Replace("-", String.Empty);

                    var admin_result = _adminService.FindByAdmin(body.username, hash_passwd);
                    if (admin_result)
                    {
                        _logger.LogInformation("Update Actication Code to DB OK");

                        result.result = "success";
                        result.description = "check super admin ok";
                    }
                    else
                    {
                        result.result = "error";
                        result.description = "SuperAdmin Invalid";
                        _logger.LogWarning("super admin invalid");
                    }
                }
                catch (Exception ex)
                {
                    ex.StackTrace.ToString();

                    result.result = "error";
                    result.description = ex.Message.ToString();
                    _logger.LogError(ex.Message.ToString());
                    return BadRequest(result);
                }
                finally
                {
                    _logger.LogInformation("---------- End CheckSuperAdmin Session() ----------");
                }
            }
            return Ok(result);
        }
    }
}

