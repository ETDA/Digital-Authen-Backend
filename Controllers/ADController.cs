using Authentication.IdentityServer.WebAPI.Models;
using Authentication.IdentityServer.WebAPI.Settings;
using hr.etda.or.th;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using static hr.etda.or.th.ServiceSoapClient;

namespace Authentication.IdentityServer.WebAPI.Controllers
{
    [ApiController]
    [Route("idp/v1")]
    //[Authorize]
    public class ADController : Controller
    {
        private readonly ILDAPSettings _ldap;
        private readonly IFidoSettings _fidoConfig;
        private readonly ILoggerFactory _loggerFactory;
        private readonly string _search_all = "";


        public ADController(ILDAPSettings ldap, ILoggerFactory loggerFactory, IFidoSettings fidoConfig)
        {
            _ldap = ldap;
            _fidoConfig = fidoConfig;
            loggerFactory =
               LoggerFactory.Create(builder =>
                   builder.AddSimpleConsole(options =>
                   {
                       options.IncludeScopes = true;
                       options.SingleLine = true;
                       options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss]: ";
                   }));
            _loggerFactory = loggerFactory;
        }

        [HttpGet("getUserInfo/{username}")]
        public ActionResult getUserInfo([FromHeader] string apikey, string username)
        {

            var _logger = _loggerFactory.CreateLogger<ADController>();

            ADUser response = new ADUser();

            if (ModelState.IsValid)
            {
                _logger.LogInformation("---------- Start Get UserInfo Session () ----------");

                ResultsModel result = new ResultsModel();

                if (apikey != _fidoConfig.API_Key)
                {
                    result.status = "error";
                    result.description = "apikey Invalid";
                    _logger.LogError(result.description);
                    return Unauthorized(result);
                }

                _logger.LogInformation("Username: " + username);

                try
                {
                    //response = GetWsdlUsers(username);
                    response = GetADUsers(username);
                    if (response == null)
                    {
                        result.status = "error";
                        result.description = "No User in AD";
                        _logger.LogError(result.description);

                        return Ok(result);
                    }

                    _logger.LogInformation("Get User Success");
                }
                catch (Exception ex)
                {
                    ex.StackTrace.ToString();

                    result.status = "error";
                    result.description = ex.Message.ToString();
                    _logger.LogError(ex.StackTrace.ToString());

                    return Ok(result);
                }
                finally
                {
                    _logger.LogInformation("---------- End Session () ----------");
                }
            }
            return Ok(response);
        }

        [Authorize]
        [HttpGet("userInfo/{username}")]
        public ActionResult getUserInfo(string username)
        {

            var _logger = _loggerFactory.CreateLogger<ADController>();
            _logger.LogInformation("---------- Start Get UserInfo Session () ----------");
            _logger.LogInformation("Username: " + username);

            ResultsModel result = new ResultsModel();
            ADUser response = new ADUser();

            try
            {
                //response = GetWsdlUsers(username);
                response = GetADUsers(username);
                if (response.EmailAddress == null)
                {
                    result.status = "error";
                    result.description = "No User in AD";
                    _logger.LogError(result.description);

                    return Ok(result);
                }

                _logger.LogInformation("Get User Success");
            }
            catch (Exception ex)
            {
                ex.StackTrace.ToString();

                result.status = "error";
                result.description = ex.Message.ToString();
                _logger.LogError(ex.StackTrace.ToString());

                return Ok(result);
            }
            finally
            {
                _logger.LogInformation("---------- End Session () ----------");
            }

            return Ok(response);
        }

        [Authorize]
        [HttpGet("GetAllProfile")]
        public ActionResult getAllUserList()
        {

            var _logger = _loggerFactory.CreateLogger<ADController>();
            _logger.LogInformation("---------- Start Get Account Session () ----------");

            ResultsModel result = new ResultsModel();
            List<ADUser> response = new List<ADUser>();

            try
            {
                //response = GetWsdlUsers();
                response = GetADUsers();
                if (response == null)
                {
                    result.status = "error";
                    result.description = "No User List from AD";
                    _logger.LogError(result.description);

                    return Ok(result);
                }

                _logger.LogInformation("Get All Account Success");
            }
            catch (Exception ex)
            {
                ex.StackTrace.ToString();

                result.status = "error";
                result.description = ex.Message.ToString();
                _logger.LogError(ex.StackTrace.ToString());

                return Ok(result);
            }
            finally
            {
                _logger.LogInformation("---------- End Session () ----------");
            }

            return Ok(response);
        }

        private List<ADUser> GetADUsers()
        {

            var _logger = _loggerFactory.CreateLogger<ADController>();

            string ldapHost = _ldap.AD_domain;
            string loginDN = _ldap.ADMIN_USER;
            string password = _ldap.ADMIN_PASS;
            string searchBase = _ldap.BASE_DN;
            string searchFilter = _ldap.SEARCH_FILTER;
            int ldapPort = _ldap.AD_port;

            string[] attrs = { "title", "mailnickname", "department", "mobiletelephonenumber", "telephonenumber", "mobile", "telephoneNumber", "cn", "distinguishedName", "sAMAccountName", "userPrincipalName", "displayName", "givenname", "sn", "mail", "mailNickname", "memberOf", "homeDirectory", "msExchUserCulture" };


            LdapConnection lc = new LdapConnection();
            List<ADUser> lstADUsers = new List<ADUser>();
            try
            {
                lc.Connect(ldapHost, ldapPort);
                lc.Bind(loginDN, password);
                _logger.LogInformation("Login OK: " + loginDN);

                LdapSearchConstraints cons = lc.SearchConstraints;
                cons.ReferralFollowing = true;
                lc.Constraints = cons;

                LdapSearchResults lsc = lc.Search(searchBase, LdapConnection.SCOPE_SUB, searchFilter, attrs, false, (LdapSearchConstraints)null);
                _logger.LogInformation("Get Account Start");
                while (lsc.HasMore())
                {
                    LdapEntry nextEntry = null;
                    try
                    {
                        nextEntry = lsc.Next();
                    }
                    catch (LdapReferralException eR)
                    {
                        _logger.LogError(eR.Message);
                        continue;
                    }
                    catch (LdapException e)
                    {
                        _logger.LogError(e.Message);
                        continue;
                    }

                    ADUser objSurveyUsers = new ADUser();

                    LdapAttribute atGN = null;
                    LdapAttribute atSN = null;
                    LdapAttribute atMail = null;
                    LdapAttribute atPhone = null;
                    LdapAttribute atDept = null;
                    LdapAttribute atTitle = null;

                    atGN = nextEntry.getAttribute("givenname");
                    atSN = nextEntry.getAttribute("sn");
                    atMail = nextEntry.getAttribute("mail");
                    atPhone = nextEntry.getAttribute("mobile");
                    atTitle = nextEntry.getAttribute("title");
                    atDept = nextEntry.getAttribute("department");

                    if (atGN != null && atSN != null && atMail != null && atPhone != null && atDept != null && atTitle != null)
                    {
                        objSurveyUsers.GivenName = atGN.StringValue;
                        objSurveyUsers.Surname = atSN.StringValue;
                        objSurveyUsers.EmailAddress = atMail.StringValue;
                        objSurveyUsers.Title = atTitle.StringValue;
                        objSurveyUsers.PhoneNumber = atPhone.StringValue;
                        objSurveyUsers.Department = atDept.StringValue;

                        lstADUsers.Add(objSurveyUsers);
                    }
                }

                if (lstADUsers.Count == 0)
                {
                    lstADUsers = null;
                }

                _logger.LogInformation("Get Account Finish");
            }
            catch (Exception ex)
            {
                lstADUsers = null;
                ex.StackTrace.ToString();
            }
            finally
            {
                lc.Disconnect();
            }

            return lstADUsers;
        }

        private ADUser GetADUsers(string username)
        {

            var _logger = _loggerFactory.CreateLogger<ADController>();

            ADUser objSurveyUsers = new ADUser();

            string ldapHost = _ldap.AD_domain;
            string loginDN = _ldap.ADMIN_USER;
            string password = _ldap.ADMIN_PASS;
            string searchBase = _ldap.BASE_DN;
            string searchFilter = _ldap.SEARCH_FILTER;
            int ldapPort = _ldap.AD_port;


            string[] attrs = { "title", "mailnickname", "department", "mobiletelephonenumber", "telephonenumber", "mobile", "telephoneNumber", "cn", "distinguishedName", "sAMAccountName", "userPrincipalName", "displayName", "givenname", "sn", "mail", "mailNickname", "memberOf", "homeDirectory", "msExchUserCulture" };

            LdapConnection lc = new LdapConnection();

            try
            {
                lc.Connect(ldapHost, ldapPort);
                lc.Bind(loginDN, password);
                _logger.LogInformation("Login OK: " + loginDN);

                LdapSearchConstraints cons = lc.SearchConstraints;
                cons.ReferralFollowing = true;
                lc.Constraints = cons;

                LdapSearchResults lsc = lc.Search(searchBase, LdapConnection.SCOPE_SUB, searchFilter, attrs, false, (LdapSearchConstraints)null);
                _logger.LogInformation("Get Account Start");
                while (lsc.HasMore())
                {
                    LdapEntry nextEntry = null;
                    try
                    {
                        nextEntry = lsc.Next();
                    }
                    catch (LdapReferralException eR)
                    {
                        _logger.LogError(eR.Message);
                        continue;
                    }
                    catch (LdapException e)
                    {
                        _logger.LogError(e.Message);
                        continue;
                    }

                    LdapAttribute atMail = null;
                    atMail = nextEntry.getAttribute("mail");

                    if (atMail != null && atMail.StringValue.Equals(username))
                    {
                        LdapAttribute atGN = null;
                        LdapAttribute atSN = null;
                        LdapAttribute atPhone = null;
                        LdapAttribute atDept = null;
                        LdapAttribute atTitle = null;

                        atGN = nextEntry.getAttribute("givenname");
                        atSN = nextEntry.getAttribute("sn");
                        atTitle = nextEntry.getAttribute("title");
                        atPhone = nextEntry.getAttribute("mobile");
                        atDept = nextEntry.getAttribute("department");

                        if (atGN != null && atSN != null && atPhone != null && atDept != null && atTitle != null)
                        {
                            objSurveyUsers.GivenName = atGN.StringValue;
                            objSurveyUsers.Surname = atSN.StringValue;
                            objSurveyUsers.Title = atTitle.StringValue;
                            objSurveyUsers.EmailAddress = atMail.StringValue;
                            objSurveyUsers.PhoneNumber = atPhone.StringValue;
                            objSurveyUsers.Department = atDept.StringValue;
                        }
                        break;
                    }
                }

                _logger.LogInformation("Get Account Finish");
            }
            catch (Exception ex)
            {
                //lstADUsers = null;
                objSurveyUsers = null;
                ex.StackTrace.ToString();
            }
            finally
            {
                lc.Disconnect();
            }

            return objSurveyUsers;
        }

        #region wsdl Method
        /*private List<ADUser> GetWsdlUsers()
        {

            List<ADUser> lstADUsers = new List<ADUser>();

            ServiceSoapClient client = new ServiceSoapClient(EndpointConfiguration.ServiceSoap);
            Task<InquiryPersonnelInfoResponse> resp = client.InquiryPersonnelInfoAsync(_search_all, _search_all, _search_all, _search_all, _search_all, _search_all);

            InquiryPersonnelInfoResponse results = resp.Result;
            var childNodes = results.Body.InquiryPersonnelInfoResult.ChildNodes;

            foreach (XmlNode xn in childNodes)
            {
                if (!xn.InnerXml.Contains("<endDT>"))
                {
                    ADUser user = new ADUser();
                    XmlNode itemnode = xn;
                    foreach (XmlNode xn1 in itemnode)
                    {
                        var key = xn1.Name;

                        switch (key)
                        {
                            case "staffID":
                                user.EmployeeId = xn1.InnerText;
                                break;
                            case "staffNameEN":
                                user.GivenName = xn1.InnerText;
                                break;
                            case "staffSurnameEN":
                                user.Surname = xn1.InnerText;
                                break;
                            case "depNameEN":
                                user.Department = xn1.InnerText;
                                break;
                            case "telephone":
                                user.PhoneNumber = xn1.InnerText;
                                break;
                            case "email":
                                user.EmailAddress = xn1.InnerText;
                                break;
                            default:
                                break;
                        }
                    }
                    lstADUsers.Add(user);
                }
            }

           *//* ADUser test = new ADUser();
            test.EmployeeId = "0001";
            test.GivenName = "Kod";
            test.Surname = "Kod";
            test.Department = "Digital Service Security Center";
            test.EmailAddress = "kod@finema.co";
            test.PhoneNumber = "1112";
            lstADUsers.Add(test);*//*

            return lstADUsers;
        }*/
        #endregion

        #region wsdl Method User Info
        /*private ADUser GetWsdlUsers(string username)
        {
            var name = username.Split("@")[0];

            if (name.Contains("."))
            {
                name = username.Split(".")[0];
            }

            ADUser user = new ADUser();

            ServiceSoapClient client = new ServiceSoapClient(EndpointConfiguration.ServiceSoap);
            Task<InquiryPersonnelInfoResponse> resp = client.InquiryPersonnelInfoAsync(_search_all, name, _search_all, _search_all, _search_all, _search_all);

            InquiryPersonnelInfoResponse results = resp.Result;
            var childNodes = results.Body.InquiryPersonnelInfoResult.ChildNodes;

            foreach (XmlNode xn in childNodes)
            {
                if (!xn.InnerXml.Contains("<endDT>"))
                {
                    XmlNode itemnode = xn;

                    if (itemnode.InnerText.Contains(username))
                    {
                        foreach (XmlNode xn1 in itemnode)
                        {
                            var key = xn1.Name;

                            switch (key)
                            {
                                case "staffID":
                                    user.EmployeeId = xn1.InnerText;
                                    break;
                                case "staffNameEN":
                                    user.GivenName = xn1.InnerText;
                                    break;
                                case "staffSurnameEN":
                                    user.Surname = xn1.InnerText;
                                    break;
                                case "depNameEN":
                                    user.Department = xn1.InnerText;
                                    break;
                                case "telephone":
                                    user.PhoneNumber = xn1.InnerText;
                                    break;
                                case "email":
                                    user.EmailAddress = xn1.InnerText;
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    }
                }
            }

            return user;
        }*/
        #endregion
    }
}
