using Authentication.IdentityServer.WebAPI.Db;
using Authentication.IdentityServer.WebAPI.Models;
using Authentication.IdentityServer.WebAPI.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using SigningServer_TedaSign.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.IdentityServer.WebAPI.Controllers
{
    [ApiController]
    [Route("idp/v1")]
    [Authorize]
    public class EmailController : Controller
    {
        private readonly ISMTPSettings _smtp;
        private readonly ILoggerFactory _loggerFactory;
        private readonly UserService _userService;

        public EmailController(ISMTPSettings smtp, UserService userService, ILoggerFactory loggerFactory)
        {
            _smtp = smtp;
            _userService = userService;

            loggerFactory =
               LoggerFactory.Create(builder =>
                   builder.AddSimpleConsole(options =>
                   {
                       options.IncludeScopes = true;
                       options.SingleLine = true;
                       options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss]: ";
                   }));
            this._loggerFactory = loggerFactory;
        }

        [HttpPost("SendEmail")]
        public ActionResult sendEmail([FromBody] EmailRequest body)
        {
            var _logger = _loggerFactory.CreateLogger<EmailController>();
            var digital_authen_email = "digitalauthen@etda.or.th";
            EmailResponse result = new EmailResponse();

            if (ModelState.IsValid)
            {
                using var smtp = new SmtpClient();

                try
                {
                    _logger.LogInformation("---------- Start Send Email Session() ----------");
                    //create email message
                    var email = new MimeMessage();
                    email.From.Add(MailboxAddress.Parse(digital_authen_email));
                    email.To.Add(MailboxAddress.Parse(body.to_address));
                    email.Subject = body.subject;
                    email.Body = new TextPart(TextFormat.Plain) { Text = string.Format("Activation Code for Registration: {0}", body.activation_code) };

                    // send email
                    
                    smtp.Connect(_smtp.SMTP_domain, _smtp.SMTP_port, SecureSocketOptions.None);
                    //smtp.Authenticate("user", "pass");
                    smtp.Send(email);
                    smtp.Disconnect(true);
                    _logger.LogInformation("Activation Code: " + body.activation_code);
                    _logger.LogInformation("Admin user: " + body.from_address);
                    _logger.LogInformation("Send Email from " + digital_authen_email + " to " + body.to_address + " SUCCESS");
                    
                    DateTime created = DateTime.Now;
                    DateTime expired = created.AddDays(1);
                    long unix_created = ((DateTimeOffset)created).ToUnixTimeSeconds();
                    long unix_expired = ((DateTimeOffset)expired).ToUnixTimeSeconds();

                    User user = new User();
                    user.admin = body.from_address;
                    user.user = body.to_address;
                    user.status = "N";
                    user.activation_code = body.activation_code;
                    user.create_date = unix_created;
                    user.expire_date = unix_expired;

                    var get_user = _userService.FindByUser(body.to_address);
                    if (get_user == null)
                    {
                        _userService.Create(user);
                        _logger.LogInformation("Insert Activation Code to DB OK");
                    }
                    else
                    {
                        user.Id = get_user.Id;
                        _userService.Update(get_user.Id, user);
                        _logger.LogInformation("Update Actication Code to DB OK");
                    }

                    result.result = "success";
                    result.description = "Email was sent";
                }
                catch (Exception ex)
                {
                    smtp.Disconnect(true);
                    _logger.LogError("Send Email ERROR: " + ex.Message);
                    ex.StackTrace.ToString();

                    result.result = "error";
                    result.description = ex.Message.ToString();
                }
                finally
                {
                    _logger.LogInformation("---------- End Send Email Session() ----------");
                }
            }
            return Ok(result);
        }
    }
}
