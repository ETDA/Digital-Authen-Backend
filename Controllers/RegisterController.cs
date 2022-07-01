using Authentication.IdentityServer.WebAPI.Db;
using Authentication.IdentityServer.WebAPI.Models;
using Authentication.IdentityServer.WebAPI.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SigningServer_TedaSign.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Authentication.IdentityServer.WebAPI.Controllers
{
    [ApiController]
    [Route("idp/v1")]
    public class RegisterController : Controller
    {
        private readonly ILDAPSettings _ldap;
        private readonly IFidoSettings _fidoConfig;
        private readonly ILoggerFactory _loggerFactory;
        private readonly UserService _userService;

        private static readonly string SUCCESS = "success";

        public RegisterController(ILDAPSettings ldap, ILoggerFactory loggerFactory, UserService userService, IFidoSettings fidoConfig)
        {
            _ldap = ldap;
            _userService = userService;
            loggerFactory =
               LoggerFactory.Create(builder =>
                   builder.AddSimpleConsole(options =>
                   {
                       options.IncludeScopes = true;
                       options.SingleLine = true;
                       options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss]: ";
                   }));
            _loggerFactory = loggerFactory;
            _fidoConfig = fidoConfig;
        }


        [HttpPost("CheckActivateCode")]
        public ActionResult checkActivationCode([FromHeader] string apikey, [FromBody] ActivateCodeRequest body)
        {
            var _logger = _loggerFactory.CreateLogger<RegisterController>();
            ActivateCodeResponse result = new ActivateCodeResponse();

            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("---------- Start CheckActivateCode Session() ----------");

                    if (apikey != _fidoConfig.API_Key)
                    {
                        result.result = "error";
                        result.description = "apikey Invalid";
                        _logger.LogError(result.description);
                        return Unauthorized(result);
                    }


                    _logger.LogInformation("Username: " + body.username);
                    _logger.LogInformation("Activation Code: " + body.activation_code);

                    var get_user = _userService.FindByUser(body.username);
                    if (get_user != null)
                    {
                        if (get_user.activation_code.Equals(body.activation_code))
                        {
                            if (get_user.status.Equals("N"))
                            {

                                DateTime now = DateTime.Now;
                                long unix_now = ((DateTimeOffset)now).ToUnixTimeSeconds();
                                if (unix_now <= get_user.expire_date)
                                {
                                    result.result = "success";
                                    result.description = "ok";
                                    _logger.LogInformation("Actication Code Valid");
                                }
                                else
                                {
                                    result.result = "error";
                                    result.description = "The Activation Code was expired";
                                    _logger.LogError("The Activation Code was expired");
                                }
                            }
                            else
                            {
                                result.result = "error";
                                result.description = "The Activation Code was used";
                                _logger.LogError("The Activation Code was used");
                            }
                        }
                        else
                        {
                            result.result = "error";
                            result.description = "Activation Code Invalid";
                            _logger.LogError("Activation Code Invalid");
                        }
                    }
                    else
                    {
                        result.result = "error";
                        result.description = "Contact Admin to Create Activation Code";
                        _logger.LogError("Contact Admin to Create Activation Code");
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
                    _logger.LogInformation("---------- End CheckActivateCode Session() ----------");
                }

            }
            return Ok(result);
        }

        [HttpPut("UpdateActCodeStatus")]
        public ActionResult updateActCodeStatus([FromHeader] string apikey, [FromBody] UpdateStatusRequest body)
        {
            var _logger = _loggerFactory.CreateLogger<RegisterController>();

            UpdateStatusResponse result = new UpdateStatusResponse();

            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("---------- Start UpdateActCodeStatus Session() ----------");

                    if (apikey != _fidoConfig.API_Key)
                    {
                        result.result = "error";
                        result.description = "apikey Invalid";
                        _logger.LogError(result.description);
                        return Unauthorized(result);
                    }

                    _logger.LogInformation("Username: " + body.username);

                    var get_user = _userService.FindByUser(body.username);
                    if (get_user != null)
                    {
                        User user = new User();
                        user.Id = get_user.Id;
                        user.admin = get_user.admin;
                        user.user = get_user.user;
                        user.status = "Y";
                        user.activation_code = get_user.activation_code;
                        user.create_date = get_user.create_date;
                        user.expire_date = get_user.expire_date;

                        _userService.Update(get_user.Id, user);
                        _logger.LogInformation("Update Actication Code to DB OK");

                        result.result = "success";
                        result.description = "update ok";
                    }
                    else
                    {
                        result.result = "error";
                        result.description = "Contact Admin to Create Activation Code";
                        _logger.LogError("Contact Admin to Create Activation Code");
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
                    _logger.LogInformation("---------- End UpdateActCodeStatus Session() ----------");
                }
            }
            return Ok(result);
        }

        [HttpGet("checkFidoStatus")]
        [Authorize]
        public ActionResult checkFidoStatus([FromBody] CheckFidoStatusRequest body)
        {
            var _logger = _loggerFactory.CreateLogger<RegisterController>();

            CheckFidoStatusResponse chk_resp = new CheckFidoStatusResponse();

            try
            {

                _logger.LogInformation("---------- Start CheckFidoStatus Session() ----------");

                _logger.LogInformation("Username: " + body.username);

                _logger.LogInformation("Curl to: " + _fidoConfig.CheckStatus_URL);

                var chk_req_body = new CheckFidoStatusRequest();
                chk_req_body.username = body.username;

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", _fidoConfig.Credentials);
                client.Timeout = TimeSpan.FromMinutes(1);
                var response = client.PostAsJsonAsync(_fidoConfig.CheckStatus_URL, chk_req_body).Result;

                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    chk_resp = JsonConvert.DeserializeObject<CheckFidoStatusResponse>(result);

                    if (chk_resp.status.Equals(SUCCESS))
                    {
                        _logger.LogInformation("Check Fido Status OK: " + chk_resp.description);
                    }
                    else
                    {
                        _logger.LogError("Check Fido Status Failed: " + chk_resp.description);
                    }
                    return Ok(chk_resp);
                }
                else
                {
                    var result_error = response.Content.ReadAsStringAsync().Result;
                    chk_resp = JsonConvert.DeserializeObject<CheckFidoStatusResponse>(result_error);

                    chk_resp.status = "error";
                    chk_resp.description = chk_resp.description;
                    _logger.LogError("Check Fido Status Failed: " + response.StatusCode);

                    return BadRequest(chk_resp);
                }
            }
            catch (Exception ex)
            {
                chk_resp.status = "error";
                chk_resp.description = ex.Message.ToString();
                _logger.LogError("Check Fido Status Exception: " + ex.Message.ToString());
                ex.StackTrace.ToString();

                return StatusCode(StatusCodes.Status500InternalServerError, chk_resp);
            }
            finally
            {
                _logger.LogInformation("---------- End CheckFidoStatus Session() ----------");
            }
        }

        [HttpPost("setBasicAuth")]
        [Authorize]
        public ActionResult setBasicAuth([FromBody] SetBasicAuthRequest body)
        {
            var _logger = _loggerFactory.CreateLogger<RegisterController>();

            SetBasicAuthResponse chk_resp = new SetBasicAuthResponse();

            try
            {

                _logger.LogInformation("---------- Start setBasicAuth Session() ----------");

                _logger.LogInformation("Username: " + body.username);

                _logger.LogInformation("Curl to: " + _fidoConfig.SetBasicAuth_URL);

                var chk_req_body = new SetBasicAuthRequest();
                chk_req_body.username = body.username;

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", _fidoConfig.Credentials);
                client.Timeout = TimeSpan.FromMinutes(1);
                var response = client.PostAsJsonAsync(_fidoConfig.SetBasicAuth_URL, chk_req_body).Result;

                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    chk_resp = JsonConvert.DeserializeObject<SetBasicAuthResponse>(result);

                    if (chk_resp.status.Equals(SUCCESS))
                    {
                        _logger.LogInformation("Set Basic Authen Status OK: " + chk_resp.description);
                    }
                    else
                    {
                        _logger.LogError("Set Basic Authen Failed: " + chk_resp.description);
                    }
                    return Ok(chk_resp);
                }
                else
                {
                    var result_error = response.Content.ReadAsStringAsync().Result;
                    chk_resp = JsonConvert.DeserializeObject<SetBasicAuthResponse>(result_error);

                    chk_resp.status = "error";
                    chk_resp.description = chk_resp.description;
                    _logger.LogError("Set Basic Authen Failed: " + response.StatusCode);

                    return BadRequest(chk_resp);
                }
            }
            catch (Exception ex)
            {
                chk_resp.status = "error";
                chk_resp.description = ex.Message.ToString();
                _logger.LogError("Set Basic Authen Exception: " + ex.Message.ToString());
                ex.StackTrace.ToString();

                return StatusCode(StatusCodes.Status500InternalServerError, chk_resp);
            }
            finally
            {
                _logger.LogInformation("---------- End setBasicAuth Session() ----------");
            }
        }
    }
}
