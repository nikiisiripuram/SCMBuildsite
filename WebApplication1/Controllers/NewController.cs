using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Net.Mail;
using AzureClientLibrary;
using WebApplication1.Models; // Assume this namespace contains your data models and context classes
using Microsoft.AspNetCore.Http;
using System.Security.Permissions;
using System.DirectoryServices.AccountManagement;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using Microsoft.Identity.Client;

namespace WebApplication1.Controllers
{
    public class NewController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NewController> _logger;
        public string releaseRequestViewModelBrach;
        public string projectName;
        public string azPAT;
        public string deliveryPostAddress;
        public string toEmailList;
        public NewController(IConfiguration configuration, ILogger<NewController> logger)
        {
            _configuration = configuration;
            _logger = logger;
            projectName = "BrassRing-Development";
            azPAT = "r3o2pxqi2euntvbtynmmxnmmvmtpz2lrpcqm7ejeheuiqbgdcqfq";
            deliveryPostAddress = "nalb-cloudops-opemail.vip.pa-dev.knxa";
            toEmailList = "infinite-talent-engineering-br-release-notification-internal@infinite.com";

        }

        [HttpGet]
        public IActionResult Index(string iid)
        {
            var releaseRequestViewModel = new ReleaseRequestViewModel();
            string buildLbl = string.Empty;
            string branch = string.Empty;
            string targetEnv = string.Empty;
            string email = UserPrincipal.Current.EmailAddress;
            ViewBag.Email = email;

            if (!string.IsNullOrEmpty(iid))
            {
                string conStr = _configuration.GetConnectionString("DefaultConnection");
                string strSQL = "SELECT * FROM tblBuild WHERE IID = @IID";

                using (SqlConnection con = new SqlConnection(conStr))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(strSQL, con))
                    {
                        cmd.Parameters.AddWithValue("@IID", iid);
                        SqlDataReader dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            buildLbl = dr["BuildNewStyle"].ToString();
                            branch = dr["Branch"].ToString();
                        }
                    }
                    con.Close();
                }

                releaseRequestViewModel.Branch = branch;
                releaseRequestViewModel.TargetEnv = targetEnv;
                releaseRequestViewModel.BuildLabel = buildLbl;

                HttpContext.Session.SetString("IID", iid);
                ViewBag.Title = "Code Release Request";
                ViewBag.NonCodeRelease = "NO";
            }
            else
            {
                ViewBag.NonCodeRelease = "YES";
                ViewBag.Title = "None-Code Release Request";
                HttpContext.Session.SetString("IID", string.Empty);
            }

            HttpContext.Session.SetString("WorkItemsSI", string.Empty);
            HttpContext.Session.SetString("WorkItemsDBScripts", string.Empty);
            return View(releaseRequestViewModel);
        }

        [HttpPost]
        public IActionResult Index(IFormCollection form)
        {
            string workitems = form["workItems"];
            string contactEmail = form["txtEmail"];
            string targetEnv = form["targetEnv"];
            string buildLbl = form["buildLblHidden"];
            string branch = form["branchHidden"];

            HttpContext.Session.SetString("WorkItemsDBScripts", HttpContext.Session.GetString("WorkItemsDBScripts"));
            HttpContext.Session.SetString("WorkItemsSI", HttpContext.Session.GetString("WorkItemsSI"));

            if (string.IsNullOrEmpty(buildLbl))
            {
                ViewBag.NonCodeRelease = "YES";
                ViewBag.Title = "None-Code Release Request";
            }
            else
            {
                ViewBag.NonCodeRelease = "NO";
                ViewBag.Title = "Code Release Request";
            }

            string finalSubmition = ReleaseRequestSubmit(workitems, contactEmail, buildLbl, branch, targetEnv);

            ViewBag.FinalSubmition = finalSubmition;
            return View();
        }

        private string ReleaseRequestSubmit(string workitems, string contactEmail, string buildLbl, string branch, string targetEnv)
        {
            //var boardsWIObj = new BoardsWorkItems(_configuration["AZProjectName"], _configuration["AZPAT"]);
            var boardsWIObj = new BoardsWorkItems(projectName, azPAT);
            tblBuildDBScriptsUpdate(workitems, contactEmail, buildLbl, branch, targetEnv);

            workitems = workitems.Replace(" ", "");
            string finalsubmit = boardsWIObj.updateTicketsInReleaseRequest(workitems, buildLbl);

            if (finalsubmit == "NOERROR")
            {
                SendReleaseMail(contactEmail, workitems, buildLbl, branch, targetEnv);
            }
            return finalsubmit;
        }

        [HttpGet]
        public ArrayList ValidateWorkItems(string workItems, string branch)
        {
            string WIDBScriptsValidation = string.Empty;
            string WorkItemsDBScripts = string.Empty;
            string InvalidSubmition = string.Empty;
            string WIValidation = string.Empty;
            string WorkItemsSI = string.Empty;
            string buildLbl = string.Empty;

            ArrayList outAry = new ArrayList();

            var boardsWIObj1 = new BoardsWorkItems(projectName, azPAT);
            //var boardsWIObj1 = new BoardsWorkItems(_configuration["AZProjectName"], _configuration["AZPAT"]);
            workItems = workItems.Replace(" ", "");
            WIValidation = boardsWIObj1.validateTicketListForReleaseRequest(workItems, branch);

            if (WIValidation.Equals("NOERROR"))
            {
                WorkItemsDBScripts = boardsWIObj1.getDBScriptsByTickets(workItems);
                if (!string.IsNullOrEmpty(WorkItemsDBScripts))
                {
                    WorkItemsDBScripts = WorkItemsDBScripts.Replace(";", ", ");
                    string strSQL = "SELECT statustypeid, SRC_FileName FROM DMCR_RequestTracker WITH (NOLOCK) WHERE SRC_FileName IN (@WorkItemsDBScripts)";
                    string conStr = _configuration.GetConnectionString("DMCRDBConnectionString");

                    using (SqlConnection con = new SqlConnection(conStr))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand(strSQL, con))
                        {
                            cmd.Parameters.AddWithValue("@WorkItemsDBScripts", WorkItemsDBScripts);
                            SqlDataReader dr = cmd.ExecuteReader();
                            while (dr.Read())
                            {
                                buildLbl = dr["BuildNewStyle"].ToString();
                                branch = dr["Branch"].ToString();
                                string ScriptStatus = dr["statustypeid"].ToString().Trim();
                                string ScriptName = dr["SRC_FileName"].ToString().Trim();
                                if (ScriptStatus.Equals("7"))
                                {
                                    WIDBScriptsValidation = "<br>There is a WI Ticket with Database Script: " + ScriptName + " attached that has not yet been approved for Release by the DBA Team";
                                    InvalidSubmition = "DBSRPT-INVALID";
                                }
                            }
                        }
                        con.Close();
                    }

                    if (!string.IsNullOrEmpty(WIDBScriptsValidation))
                    {
                        WorkItemsDBScripts = WIDBScriptsValidation;
                    }
                }
                else
                {
                    WorkItemsDBScripts = "No DB Scripts";
                }

                if (InvalidSubmition == "DBSRPT-INVALID")
                {
                    outAry.Add(InvalidSubmition + "|||" + WIDBScriptsValidation);
                }
                else
                {
                    outAry.Add("DBSRPT-VALID|||" + WorkItemsDBScripts);
                    HttpContext.Session.SetString("WorkItemsDBScripts", WorkItemsDBScripts);
                }

                WorkItemsSI = boardsWIObj1.getSpecialInstructionsFromTickets(workItems);
                if (string.IsNullOrEmpty(WorkItemsSI))
                {
                    WorkItemsSI = "NO SI";
                }
                outAry.Add("WI-SI|||" + WorkItemsSI);
                HttpContext.Session.SetString("WorkItemsSI", WorkItemsSI);
            }
            else
            {
                outAry.Add("WI-INVALID|||" + WIValidation);
            }

            return outAry;
        }

        private void tblBuildDBScriptsUpdate(string workitems, string contactEmail, string buildLbl, string branch, string targetEnv)
        {
            string conStr = _configuration.GetConnectionString("DefaultConnection");
            string currentTime = DateTime.Now.ToString();
            string targetEnv2 = targetEnv.Replace(" Now", "").Replace(" Upon QA Approval", "").Replace("EU-Prod", "Prod").Replace("US-Prod", "Prod").Replace("One-Stg", "Prod");
            string NCIID = string.Empty;
            string tempSI1 = string.Empty;

            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();
                try
                {
                    if (ViewBag.NonCodeRelease == "YES")
                    {
                        string krbPrefix = string.Empty;
                        string nCRelaseProduct = Request.Form["appInfo"];

                        krbPrefix = nCRelaseProduct.Contains("KRBUtilities") ? string.Empty : "KRB";

                        if (targetEnv2.Contains(","))
                        {
                            targetEnv2 = targetEnv2.Split(',')[0];
                        }

                        branch = nCRelaseProduct + "-" + targetEnv2;

                        string strSQL = "INSERT INTO tblBuild (Project, Branch, BranchFloating, Label, DateAdded, BuildActive, RequesterEmail, EmailList, ConfigChanges, SpecialInstructions, nonCode, DateReleaseRequested) " +
                                        "VALUES (@Project, @Branch, @BranchFloating, @Label, @DateAdded, @BuildActive, @RequesterEmail, @EmailList, @ConfigChanges, @SpecialInstructions, @nonCode, @DateReleaseRequested)";

                        using (SqlCommand cmd = new SqlCommand(strSQL, con))
                        {
                            cmd.Parameters.AddWithValue("@Project", krbPrefix + nCRelaseProduct.Replace("-", ""));
                            cmd.Parameters.AddWithValue("@Branch", currentTime);
                            cmd.Parameters.AddWithValue("@BranchFloating", nCRelaseProduct + "-" + targetEnv2);
                            cmd.Parameters.AddWithValue("@Label", currentTime);
                            cmd.Parameters.AddWithValue("@DateAdded", currentTime);
                            cmd.Parameters.AddWithValue("@BuildActive", 1);
                            cmd.Parameters.AddWithValue("@RequesterEmail", contactEmail);
                            cmd.Parameters.AddWithValue("@EmailList", contactEmail);
                            cmd.Parameters.AddWithValue("@ConfigChanges", string.Empty);
                            cmd.Parameters.AddWithValue("@SpecialInstructions", tempSI1);
                            cmd.Parameters.AddWithValue("@nonCode", 1);
                            cmd.Parameters.AddWithValue("@DateReleaseRequested", currentTime);
                            cmd.ExecuteNonQuery();
                        }

                        strSQL = "SELECT IID, BuildOldStyle FROM tblBuild WHERE Project = @Project AND Branch = @Branch AND BranchFloating = @BranchFloating";

                        using (SqlCommand cmd = new SqlCommand(strSQL, con))
                        {
                            cmd.Parameters.AddWithValue("@Project", krbPrefix + nCRelaseProduct.Replace("-", ""));
                            cmd.Parameters.AddWithValue("@Branch", currentTime);
                            cmd.Parameters.AddWithValue("@BranchFloating", nCRelaseProduct + "-" + targetEnv2);
                            SqlDataReader dr1 = cmd.ExecuteReader();
                            if (dr1.Read())
                            {
                                buildLbl = dr1["BuildOldStyle"].ToString();
                                NCIID = dr1["IID"].ToString();
                                buildLbl = buildLbl.Replace(".", " NC ");
                            }
                        }
                    }
                    else
                    {
                        string iid = HttpContext.Session.GetString("IID");

                        string strSQL = "UPDATE tblBuild SET " +
                                        "RequesterEmail = @RequesterEmail, " +
                                        "EmailList = @EmailList, " +
                                        "ConfigChanges = @ConfigChanges, " +
                                        "SpecialInstructions = @SpecialInstructions, " +
                                        "DateReleaseRequested = @DateReleaseRequested " +
                                        "WHERE IID = @IID";

                        using (SqlCommand cmd = new SqlCommand(strSQL, con))
                        {
                            cmd.Parameters.AddWithValue("@RequesterEmail", contactEmail);
                            cmd.Parameters.AddWithValue("@EmailList", contactEmail);
                            cmd.Parameters.AddWithValue("@ConfigChanges", string.Empty);
                            cmd.Parameters.AddWithValue("@SpecialInstructions", tempSI1);
                            cmd.Parameters.AddWithValue("@DateReleaseRequested", currentTime);
                            cmd.Parameters.AddWithValue("@IID", iid);
                            cmd.ExecuteNonQuery();
                        }

                        strSQL = "SELECT IID FROM tblBuildDBScripts WHERE IID = @IID";
                        using (SqlCommand cmd = new SqlCommand(strSQL, con))
                        {
                            cmd.Parameters.AddWithValue("@IID", iid);
                            SqlDataReader dr2 = cmd.ExecuteReader();
                            if (dr2.Read())
                            {
                                strSQL = "UPDATE tblBuildDBScripts SET DBScripts = @DBScripts WHERE IID = @IID";
                            }
                            else
                            {
                                strSQL = "INSERT INTO tblBuildDBScripts (IID, DBScripts) VALUES (@IID, @DBScripts)";
                            }
                        }

                        using (SqlCommand cmd = new SqlCommand(strSQL, con))
                        {
                            cmd.Parameters.AddWithValue("@IID", iid);
                            cmd.Parameters.AddWithValue("@DBScripts", HttpContext.Session.GetString("WorkItemsDBScripts"));
                            cmd.ExecuteNonQuery();
                        }
                    }

                    con.Close();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating tblBuildDBScripts");
                    SendErrorMail(ex.Message, contactEmail, workitems, branch, buildLbl, targetEnv);
                }
            }
        }

        private void SendReleaseMail(string contactEmail, string workitems, string buildLbl, string branch, string targetEnv)
        {
            StringBuilder emailBody = new StringBuilder();
            emailBody.Append("<table border=1 bordercolor=gray bgcolor=#d3d3d3>");
            emailBody.Append($"<tr><td>Contact-Info</td><td>{contactEmail}</td></tr>");
            emailBody.Append($"<tr><td>Branch</td><td>{branch}</td></tr>");
            emailBody.Append($"<tr><td>Build</td><td>{buildLbl}</td></tr>");
            emailBody.Append($"<tr><td>Work-Items</td><td>{workitems}</td></tr>");
            emailBody.Append($"<tr><td>DB-Scripts</td><td>{HttpContext.Session.GetString("WorkItemsDBScripts")}</td></tr>");
            emailBody.Append($"<tr><td>Release-SI</td><td><pre>{HttpContext.Session.GetString("WorkItemsSI")}</pre></td></tr>");
            emailBody.Append($"<tr><td>Target-Env</td><td>{targetEnv.Replace(",", "<br>")}</td></tr>");
            emailBody.Append("</table>");

            string toEmailList = _configuration["ToEmailList"];

            try
            {
                MailMessage oMailMessage = new MailMessage("donotreply@brassring.com", contactEmail + "," + toEmailList)
                {
                    SubjectEncoding = Encoding.UTF8,
                    BodyEncoding = Encoding.UTF8,
                    IsBodyHtml = true,
                    Subject = $"Release Request - {contactEmail} for {buildLbl}",
                    Body = emailBody.ToString()
                };

                SmtpClient oSmtpClient = new SmtpClient
                {
                    Host = "smtphost.brassring.com",
                    Port = 25,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                oSmtpClient.Send(oMailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending release email");
            }
        }

        private void SendErrorMail(string errorMsg, string contactEmail, string workitems, string branch, string buildLbl, string targetEnv)
        {
            StringBuilder emailBody = new StringBuilder();
            emailBody.Append("<p>An error occurred during the submission:</p>");
            emailBody.Append("<p>" + errorMsg + "</p>");
            emailBody.Append("<p>Details:</p>");
            emailBody.Append("<table border=1 bordercolor=gray bgcolor=#d3d3d3>");
            emailBody.Append($"<tr><td>Contact-Info</td><td>{contactEmail}</td></tr>");
            emailBody.Append($"<tr><td>Branch</td><td>{branch}</td></tr>");
            emailBody.Append($"<tr><td>Build</td><td>{buildLbl}</td></tr>");
            emailBody.Append($"<tr><td>Work-Items</td><td>{workitems}</td></tr>");
            emailBody.Append($"<tr><td>DB-Scripts</td><td>{HttpContext.Session.GetString("WorkItemsDBScripts")}</td></tr>");
            emailBody.Append($"<tr><td>Release-SI</td><td><pre>{HttpContext.Session.GetString("WorkItemsSI")}</pre></td></tr>");
            emailBody.Append($"<tr><td>Target-Env</td><td>{targetEnv.Replace(",", "<br>")}</td></tr>");
            emailBody.Append("</table>");

            string errorEmailList = _configuration["ErrorEmailList"];

            try
            {
                MailMessage oMailMessage = new MailMessage("donotreply@brassring.com", errorEmailList)
                {
                    SubjectEncoding = Encoding.UTF8,
                    BodyEncoding = Encoding.UTF8,
                    IsBodyHtml = true,
                    Subject = $"Error in Release Request - {contactEmail} for {buildLbl}",
                    Body = emailBody.ToString()
                };

                SmtpClient oSmtpClient = new SmtpClient
                {
                    Host = "smtphost.brassring.com",
                    Port = 25,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                oSmtpClient.Send(oMailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending error email");
            }
        }

    }
}

