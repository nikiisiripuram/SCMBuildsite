
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;
//using Microsoft.AspNetCore.Http;
//using System;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Net.Mail;
//using System.Text;
//using AzureClientLibrary;

//namespace WebApplication1.Controllers
//{
//    public class ReleaseRequestController : Controller
//    {
//        private readonly IConfiguration _configuration;

//        public ReleaseRequestController(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }

//        public static string BuildLbl { get; set; }
//        public static string Branch { get; set; }
//        public static string TargetEnv { get; set; }
//        public static string WorkItemsSI { get; set; }
//        public static string WorkItemsDBScripts { get; set; }
//        public static string InvalidSubmission { get; set; }
//        public static string WIValidation { get; set; }
//        public string FinalSubmission { get; set; }
//        public string NonCodeRelease { get; set; }
//        public string Title { get; set; }
//        public string NCReleaseProduct { get; set; }

//        [HttpGet]
//        public IActionResult Index(string IID)
//        {
//            if (!string.IsNullOrEmpty(IID))
//            {
//                var conStr = _configuration.GetConnectionString("tdBrassringQAConnectionString");
//                var strSQL = "Select * from tblBuild where IID=" + IID;
//                using (var con = new SqlConnection(conStr))
//                {
//                    con.Open();
//                    using (var cmd = new SqlCommand(strSQL, con))
//                    {
//                        var dr = cmd.ExecuteReader();
//                        if (dr.Read())
//                        {
//                            BuildLbl = dr["BuildNewStyle"].ToString();
//                            Branch = dr["Branch"].ToString();
//                        }
//                    }
//                }
//                Title = "Code Release Request";
//                NonCodeRelease = "NO";
//                HttpContext.Session.SetString("IID", IID);
//            }
//            else
//            {
//                BuildLbl = "";
//                Branch = "";
//                NonCodeRelease = "YES";
//                Title = "None-Code Release Request";
//                HttpContext.Session.SetString("IID", "");
//            }
//            HttpContext.Session.SetString("WorkItemsSI", "");
//            HttpContext.Session.SetString("WorkItemsDBScripts", "");

//            return View();
//        }

//        [HttpPost]
//        public IActionResult Submit(string workItems, string txtEmail, string targetEnv, string buildLblHidden, string branchHidden, string appInfo)
//        {
//            if (ModelState.IsValid)
//            {
//                var siValidationResult = ValidateSI(workItems);
//                if (siValidationResult == "NO")
//                {
//                    ModelState.AddModelError("", "Special Instructions contain disallowed phrases.");
//                    return View("Index");
//                }

//                workItems = Request.Form["workItems"];
//                var contactEmail = Request.Form["txtEmail"];
//                TargetEnv = Request.Form["targetEnv"];
//                BuildLbl = Request.Form["buildLblHidden"];
//                Branch = Request.Form["branchHidden"];
//                WorkItemsDBScripts = HttpContext.Session.GetString("WorkItemsDBScripts");
//                WorkItemsSI = HttpContext.Session.GetString("WorkItemsSI");
//                var IID = HttpContext.Session.GetString("IID");

//                if (string.IsNullOrEmpty(BuildLbl))
//                {
//                    NonCodeRelease = "YES";
//                    Title = "None-Code Release Request";
//                    NCReleaseProduct = Request.Form["appInfo"];
//                }
//                else
//                {
//                    NonCodeRelease = "NO";
//                    Title = "Code Release Request";
//                }

//                FinalSubmission = ReleaseRequestSubmit(workItems, contactEmail, appInfo);
//                if (FinalSubmission.Equals("NOERROR"))
//                {
//                    return RedirectToAction("Success");
//                }
//            }

//            return View("Index");
//        }

//        public IActionResult Success()
//        {
//            return View();
//        }

//        private string ReleaseRequestSubmit(string workItems, string contactEmail, string appInfo)
//        {
//            var boardsWIObj = new BoardsWorkItems(_configuration["AZProjectName"], _configuration["AZPAT"]);
//            tblBuildDBScriptsUpdate(workItems, contactEmail, appInfo);
//            workItems = workItems.Replace(" ", "");
//            var finalsubmit = boardsWIObj.updateTicketsInReleaseRequest(workItems, BuildLbl);
//            if (finalsubmit.Equals("NOERROR"))
//            {
//                SendReleaseMail(contactEmail, workItems);
//            }
//            return finalsubmit;
//        }

//        private void tblBuildDBScriptsUpdate(string workItems, string contactEmail, string appInfo)
//        {
//            var conStr = _configuration.GetConnectionString("tdBrassringQAConnectionString");
//            var currentTime = DateTime.Now.ToString();
//            var targetEnv2 = TargetEnv.Replace(" Now", "").Replace(" Upon QA Approval", "").Replace("EU-Prod", "Prod").Replace("US-Prod", "Prod").Replace("One-Stg", "Prod");
//            var NCIID = "";
//            var tempSI1 = "";

//            using (var con = new SqlConnection(conStr))
//            {
//                con.Open();
//                try
//                {
//                    if (NonCodeRelease.Equals("YES"))
//                    {
//                        var krbPrefix = "";
//                        krbPrefix = NCReleaseProduct.Contains("KRBUtilities") ? "" : "KRB";

//                        if (targetEnv2.Contains(","))
//                        {
//                            targetEnv2 = targetEnv2.Split(',')[0];
//                        }
//                        Branch = NCReleaseProduct + "-" + targetEnv2;

//                        var strSQL = "Insert into tblBuild (Project, Branch, BranchFloating, Label, DateAdded, BuildActive, RequesterEmail, EmailList, ConfigChanges, SpecialInstructions, nonCode, DateReleaseRequested) " +
//                        "Values (@Project, @Branch, @BranchFloating, @Label, @DateAdded, 1, @RequesterEmail, @EmailList, '', @SpecialInstructions, 1, @DateReleaseRequested)";
//                        using (var cmd = new SqlCommand(strSQL, con))
//                        {
//                            cmd.Parameters.AddWithValue("@Project", krbPrefix + NCReleaseProduct.Replace("-", ""));
//                            cmd.Parameters.AddWithValue("@Branch", currentTime);
//                            cmd.Parameters.AddWithValue("@BranchFloating", NCReleaseProduct + "-" + targetEnv2);
//                            cmd.Parameters.AddWithValue("@Label", currentTime);
//                            cmd.Parameters.AddWithValue("@DateAdded", currentTime);
//                            cmd.Parameters.AddWithValue("@RequesterEmail", contactEmail);
//                            cmd.Parameters.AddWithValue("@EmailList", contactEmail);
//                            cmd.Parameters.AddWithValue("@SpecialInstructions", tempSI1);
//                            cmd.Parameters.AddWithValue("@DateReleaseRequested", currentTime);
//                            cmd.ExecuteNonQuery();
//                        }

//                        strSQL = "SELECT IID, BuildOldStyle FROM dbo.tblBuild WHERE Project = @Project AND Branch = @Branch AND BranchFloating = @BranchFloating";
//                        using (var cmd = new SqlCommand(strSQL, con))
//                        {
//                            cmd.Parameters.AddWithValue("@Project", krbPrefix + NCReleaseProduct.Replace("-", ""));
//                            cmd.Parameters.AddWithValue("@Branch", currentTime);
//                            cmd.Parameters.AddWithValue("@BranchFloating", NCReleaseProduct + "-" + targetEnv2);
//                            var dr1 = cmd.ExecuteReader();
//                            if (dr1.Read())
//                            {
//                                BuildLbl = dr1["BuildOldStyle"].ToString();
//                                NCIID = dr1["IID"].ToString();
//                                BuildLbl = BuildLbl.Replace(".", " NC ");
//                            }
//                        }
//                    }
//                    else
//                    {
//                        var strSQL = "Update tblBuild Set " +
//                                     "RequesterEmail=@RequesterEmail, " +
//                                     "EmailList=@EmailList, " +
//                                     "ConfigChanges='', " +
//                                     "SpecialInstructions=@SpecialInstructions, " +
//                                     "DateReleaseRequested=@DateReleaseRequested " +
//                                     "Where IID=@IID";
//                        using (var cmd = new SqlCommand(strSQL, con))
//                        {
//                            cmd.Parameters.AddWithValue("@RequesterEmail", contactEmail);
//                            cmd.Parameters.AddWithValue("@EmailList", contactEmail);
//                            cmd.Parameters.AddWithValue("@SpecialInstructions", tempSI1);
//                            cmd.Parameters.AddWithValue("@DateReleaseRequested", currentTime);
//                            cmd.Parameters.AddWithValue("@IID", HttpContext.Session.GetString("IID"));
//                            cmd.ExecuteNonQuery();
//                        }

//                        strSQL = "SELECT IID FROM tblBuildDBScripts WHERE IID=@IID";
//                        using (var cmd = new SqlCommand(strSQL, con))
//                        {
//                            cmd.Parameters.AddWithValue("@IID", HttpContext.Session.GetString("IID"));
//                            var dr2 = cmd.ExecuteReader();
//                            if (dr2.Read())
//                            {
//                                strSQL = "UPDATE tblBuildDBScripts SET DBScripts=@DBScripts WHERE IID=@IID";
//                            }
//                            else
//                            {
//                                strSQL = "INSERT INTO tblBuildDBScripts (IID, DBScripts) VALUES (@IID, @DBScripts)";
//                            }
//                            using (var cmd2 = new SqlCommand(strSQL, con))
//                            {
//                                cmd2.Parameters.AddWithValue("@DBScripts", WorkItemsDBScripts);
//                                cmd2.Parameters.AddWithValue("@IID", HttpContext.Session.GetString("IID"));
//                                cmd2.ExecuteNonQuery();
//                            }
//                        }
//                    }
//                }
//                catch (SqlException sqlEx)
//                {
//                    SendErrorEmail(sqlEx);
//                }
//                catch (Exception ex)
//                {
//                    SendErrorEmail(ex);
//                }
//            }
//        }

//        private void SendReleaseMail(string contactEmail, string workitems)
//        {
//            var emailBody = new StringBuilder();
//            emailBody.Append("<table border=1 bordercolor=blue width=80%>");
//            emailBody.Append("<tr><td colspan=2 align=center>Release Details</td></tr>");
//            emailBody.Append("<tr><td>Work Items:</td><td>" + workitems + "</td></tr>");
//            emailBody.Append("<tr><td>Branch:</td><td>" + Branch + "</td></tr>");
//            emailBody.Append("<tr><td>Target Environment:</td><td>" + TargetEnv + "</td></tr>");
//            emailBody.Append("<tr><td>Build Label:</td><td>" + BuildLbl + "</td></tr>");
//            emailBody.Append("</table>");

//            var email = new MailMessage
//            {
//                From = new MailAddress("release@domain.com"),
//                Subject = "Release Request Submitted",
//                Body = emailBody.ToString(),
//                IsBodyHtml = true
//            };
//            email.To.Add(contactEmail);

//            var smtp = new SmtpClient("smtp.domain.com");
//            smtp.Send(email);
//        }

//        private void SendErrorEmail(Exception ex)
//        {
//            var emailBody = new StringBuilder();
//            emailBody.Append("<table border=1 bordercolor=red width=80%>");
//            emailBody.Append("<tr><td colspan=2 align=center>Error Details</td></tr>");
//            emailBody.Append("<tr><td>Error Message:</td><td>" + ex.Message + "</td></tr>");
//            emailBody.Append("<tr><td>Stack Trace:</td><td>" + ex.StackTrace + "</td></tr>");
//            emailBody.Append("</table>");

//            var email = new MailMessage
//            {
//                From = new MailAddress("error@domain.com"),
//                Subject = "Release Request Error",
//                Body = emailBody.ToString(),
//                IsBodyHtml = true
//            };
//            email.To.Add("admin@domain.com");

//            var smtp = new SmtpClient("smtp.domain.com");
//            smtp.Send(email);
//        }

//        private string ValidateSI(string workItemsSI)
//        {
//            var disallowedPhrases = new List<string> { "install", "deploy", "invoke" };

//            foreach (var phrase in disallowedPhrases)
//            {
//                if (workItemsSI.ToLower().Contains(phrase))
//                {
//                    return "NO";
//                }
//            }

//            HttpContext.Session.SetString("WorkItemsSI", workItemsSI);
//            return "YES";
//        }
//        //public IActionResult Nikhil1()
//        //{
//        //    ViewBag.buildLbl = "Build Label";
//        //    ViewBag.finalSubmition = "NOERROR"; // Change this based on your logic
//        //    ViewBag.cloneTickets = ""; // Assign based on your logic

//        //    return View();
//        //}

//        //[HttpPost]
//        //public IActionResult Nikhil1(FormCollection collection)
//        //{
//        //    // Handle the form submission logic here

//        //    return View();
//        //}

//    }

//}







//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;
//using System.Data.SqlClient;
//using System.Net.Mail;
//using System.Text;
//using AzureClientLibrary;

//namespace WebApplication1.Controllers
//{
//    public class ReleaseRequestController : Controller
//    {
//        private readonly IConfiguration _configuration;

//        public ReleaseRequestController(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }

//        public static string BuildLbl { get; set; }
//        public static string Branch { get; set; }
//        public static string TargetEnv { get; set; }
//        public static string WorkItemsSI { get; set; }
//        public static string WorkItemsDBScripts { get; set; }
//        public static string InvalidSubmission { get; set; }
//        public static string WIValidation { get; set; }
//        public string FinalSubmission { get; set; }
//        public string NonCodeRelease { get; set; }
//        public string Title { get; set; }
//        public string NCReleaseProduct { get; set; }
//        public string CloneTickets { get; set; }

//        [HttpGet]
//        public IActionResult CodeRel(string IID)
//        {
//            if (!string.IsNullOrEmpty(IID))
//            {
//                var conStr = _configuration.GetConnectionString("DefaultConnection");
//                var strSQL = "Select * from tblBuild where IID=" + IID;
//                using (var con = new SqlConnection(conStr))
//                {
//                    con.Open();
//                    using (var cmd = new SqlCommand(strSQL, con))
//                    {
//                        var dr = cmd.ExecuteReader();
//                        if (dr.Read())
//                        {
//                            BuildLbl = dr["BuildNewStyle"].ToString();
//                            Branch = dr["Branch"].ToString();
//                        }
//                    }
//                }
//                Title = "Code Release Request";
//                NonCodeRelease = "NO";
//                HttpContext.Session.SetString("IID", IID);
//            }
//            else
//            {
//                BuildLbl = "";
//                Branch = "";
//                NonCodeRelease = "YES";
//                Title = "None-Code Release Request";
//                HttpContext.Session.SetString("IID", "");
//            }
//            HttpContext.Session.SetString("WorkItemsSI", "");
//            HttpContext.Session.SetString("WorkItemsDBScripts", "");

//            return View();
//        }

//        [HttpPost]
//        public IActionResult Submit(string workItems, string txtEmail, string targetEnv, string buildLblHidden, string branchHidden, string appInfo)
//        {
//            if (ModelState.IsValid)
//            {
//                workItems = Request.Form["workItems"];
//                var contactEmail = Request.Form["txtEmail"];
//                TargetEnv = Request.Form["targetEnv"];
//                BuildLbl = Request.Form["buildLblHidden"];
//                Branch = Request.Form["branchHidden"];
//                WorkItemsDBScripts = HttpContext.Session.GetString("WorkItemsDBScripts");
//                WorkItemsSI = HttpContext.Session.GetString("WorkItemsSI");
//                var IID = HttpContext.Session.GetString("IID");

//                if (string.IsNullOrEmpty(BuildLbl))
//                {
//                    NonCodeRelease = "YES";
//                    Title = "None-Code Release Request";
//                    NCReleaseProduct = Request.Form["appInfo"];
//                }
//                else
//                {
//                    NonCodeRelease = "NO";
//                    Title = "Code Release Request";
//                }

//                FinalSubmission = ReleaseRequestSubmit(workItems, contactEmail, appInfo);
//                if (FinalSubmission.Equals("NOERROR"))
//                {
//                    return RedirectToAction("Success");
//                }
//            }

//            return View("CodeRel");
//        }

//        public IActionResult Success()
//        {
//            return View();
//        }

//        private string ReleaseRequestSubmit(string workItems, string contactEmail, string appInfo)
//        {
//            var boardsWIObj = new BoardsWorkItems(_configuration["AZProjectName"], _configuration["AZPAT"]);
//            tblBuildDBScriptsUpdate(workItems, contactEmail, appInfo);
//            workItems = workItems.Replace(" ", "");
//            var finalsubmit = boardsWIObj.updateTicketsInReleaseRequest(workItems, BuildLbl);
//            if (finalsubmit.Equals("NOERROR"))
//            {
//                SendReleaseMail(contactEmail, workItems);
//            }
//            return finalsubmit;
//        }

//        private void tblBuildDBScriptsUpdate(string workItems, string contactEmail, string appInfo)
//        {
//            var conStr = _configuration.GetConnectionString("tdBrassringQAConnectionString");
//            var currentTime = DateTime.Now.ToString();
//            var targetEnv2 = TargetEnv.Replace(" Now", "").Replace(" Upon QA Approval", "").Replace("EU-Prod", "Prod").Replace("US-Prod", "Prod").Replace("One-Stg", "Prod");
//            var NCIID = "";
//            var tempSI1 = "";

//            using (var con = new SqlConnection(conStr))
//            {
//                con.Open();
//                try
//                {
//                    if (NonCodeRelease.Equals("YES"))
//                    {
//                        var krbPrefix = "";
//                        krbPrefix = NCReleaseProduct.Contains("KRBUtilities") ? "" : "KRB";

//                        if (targetEnv2.Contains(","))
//                        {
//                            targetEnv2 = targetEnv2.Split(',')[0];
//                        }
//                        Branch = NCReleaseProduct + "-" + targetEnv2;

//                        var strSQL = "Insert into tblBuild (Project, Branch, BranchFloating, Label, DateAdded, BuildActive, RequesterEmail, EmailList, ConfigChanges, SpecialInstructions, nonCode, DateReleaseRequested) " +
//                        "Values (@Project, @Branch, @BranchFloating, @Label, @DateAdded, 1, @RequesterEmail, @EmailList, '', @SpecialInstructions, 1, @DateReleaseRequested)";
//                        using (var cmd = new SqlCommand(strSQL, con))
//                        {
//                            cmd.Parameters.AddWithValue("@Project", krbPrefix + NCReleaseProduct.Replace("-", ""));
//                            cmd.Parameters.AddWithValue("@Branch", currentTime);
//                            cmd.Parameters.AddWithValue("@BranchFloating", NCReleaseProduct + "-" + targetEnv2);
//                            cmd.Parameters.AddWithValue("@Label", currentTime);
//                            cmd.Parameters.AddWithValue("@DateAdded", currentTime);
//                            cmd.Parameters.AddWithValue("@RequesterEmail", contactEmail);
//                            cmd.Parameters.AddWithValue("@EmailList", contactEmail);
//                            cmd.Parameters.AddWithValue("@SpecialInstructions", tempSI1);
//                            cmd.Parameters.AddWithValue("@DateReleaseRequested", currentTime);
//                            cmd.ExecuteNonQuery();
//                        }

//                        strSQL = "SELECT IID, BuildOldStyle FROM dbo.tblBuild WHERE Project = @Project AND Branch = @Branch AND BranchFloating = @BranchFloating";
//                        using (var cmd = new SqlCommand(strSQL, con))
//                        {
//                            cmd.Parameters.AddWithValue("@Project", krbPrefix + NCReleaseProduct.Replace("-", ""));
//                            cmd.Parameters.AddWithValue("@Branch", currentTime);
//                            cmd.Parameters.AddWithValue("@BranchFloating", NCReleaseProduct + "-" + targetEnv2);
//                            var dr1 = cmd.ExecuteReader();
//                            if (dr1.Read())
//                            {
//                                BuildLbl = dr1["BuildOldStyle"].ToString();
//                                NCIID = dr1["IID"].ToString();
//                                BuildLbl = BuildLbl.Replace(".", " NC ");
//                            }
//                        }
//                    }
//                    else
//                    {
//                        var strSQL = "Update tblBuild Set " +
//                                     "RequesterEmail=@RequesterEmail, " +
//                                     "EmailList=@EmailList, " +
//                                     "ConfigChanges='', " +
//                                     "SpecialInstructions=@SpecialInstructions, " +
//                                     "DateReleaseRequested=@DateReleaseRequested " +
//                                     "Where IID=@IID";
//                        using (var cmd = new SqlCommand(strSQL, con))
//                        {
//                            cmd.Parameters.AddWithValue("@RequesterEmail", contactEmail);
//                            cmd.Parameters.AddWithValue("@EmailList", contactEmail);
//                            cmd.Parameters.AddWithValue("@SpecialInstructions", tempSI1);
//                            cmd.Parameters.AddWithValue("@DateReleaseRequested", currentTime);
//                            cmd.Parameters.AddWithValue("@IID", HttpContext.Session.GetString("IID"));
//                            cmd.ExecuteNonQuery();
//                        }

//                        strSQL = "SELECT IID FROM tblBuildDBScripts WHERE IID=@IID";
//                        using (var cmd = new SqlCommand(strSQL, con))
//                        {
//                            cmd.Parameters.AddWithValue("@IID", HttpContext.Session.GetString("IID"));
//                            var dr2 = cmd.ExecuteReader();
//                            if (dr2.Read())
//                            {
//                                strSQL = "UPDATE tblBuildDBScripts SET DBScripts=@DBScripts WHERE IID=@IID";
//                            }
//                            else
//                            {
//                                strSQL = "INSERT INTO tblBuildDBScripts (IID, DBScripts) VALUES (@IID, @DBScripts)";
//                            }
//                            using (var cmd2 = new SqlCommand(strSQL, con))
//                            {
//                                cmd2.Parameters.AddWithValue("@DBScripts", WorkItemsDBScripts);
//                                cmd2.Parameters.AddWithValue("@IID", HttpContext.Session.GetString("IID"));
//                                cmd2.ExecuteNonQuery();
//                            }
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    SendErrorEmail(ex);
//                }
//            }
//        }

//        private void SendReleaseMail(string contactEmail, string workitems)
//        {
//            var emailBody = new StringBuilder();
//            emailBody.Append("<table border=1 bordercolor=blue width=80%>");
//            emailBody.Append("<tr><td colspan=2 align=center>Release Details</td></tr>");
//            emailBody.Append("<tr><td>Work Items:</td><td>" + workitems + "</td></tr>");
//            emailBody.Append("<tr><td>Branch:</td><td>" + Branch + "</td></tr>");
//            emailBody.Append("<tr><td>Build:</td><td>" + BuildLbl + "</td></tr>");
//            emailBody.Append("<tr><td>Target Environment:</td><td>" + TargetEnv + "</td></tr>");
//            emailBody.Append("<tr><td>Requestor:</td><td>" + contactEmail + "</td></tr>");
//            emailBody.Append("</table>");
//            var message = new MailMessage
//            {
//                From = new MailAddress("you@example.com"),
//                Subject = "Release Request",
//                Body = emailBody.ToString(),
//                IsBodyHtml = true
//            };
//            message.To.Add(contactEmail);
//            message.To.Add("others@example.com");

//            using (var smtp = new SmtpClient("smtp.example.com"))
//            {
//                smtp.Send(message);
//            }
//        }

//        private void SendErrorEmail(Exception ex)
//        {
//            var errorEmailBody = new StringBuilder();
//            errorEmailBody.Append("<tr><td align=center><font color=red size=5>Error Submitting Release Request Form</font></td></tr>");
//            errorEmailBody.Append("<tr><td align=center><font color=red size=5>" + ex.Message + "</font></td></tr>");
//            var errorMsg = new MailMessage
//            {
//                From = new MailAddress("you@example.com"),
//                Subject = "Error Submitting Release Request Form",
//                Body = errorEmailBody.ToString(),
//                IsBodyHtml = true
//            };
//            errorMsg.To.Add("admin@example.com");

//            using (var smtp = new SmtpClient("smtp.example.com"))
//            {
//                smtp.Send(errorMsg);
//            }
//        }
//    }
//}









////using Microsoft.AspNetCore.Mvc;

////namespace WebApplication1.Controllers
////{
////    public class ReleaseRequestController : Controller
////    {
////        [HttpGet]
////        public IActionResult ReleaseRequest()
////        {
////            ViewBag.FinalSubmission = "NOERROR"; // Replace with actual logic
////            ViewBag.BuildLbl = "Build Label"; // Replace with actual logic
////            ViewBag.Branch = "Branch"; // Replace with actual logic
////            ViewBag.CloneTickets = ""; // Replace with actual logic
////            ViewBag.Title = "Release Request"; // Replace with actual logic
////            ViewBag.NonCodeRelease = "NO"; // Replace with actual logic

////            return View();
////        }

////        [HttpPost]
////        public IActionResult Submit(ReleaseRequestModel model)
////        {
////            if (ModelState.IsValid)
////            {
////                // Handle form submission logic here
////                // For example, save data to the database, send email, etc.

////                // Redirect to a success page or return success message
////                return RedirectToAction("Success");
////            }

////            return View("Index", model);
////        }

////        public IActionResult Success()
////        {
////            return View();
////        }
////    }

////    public class ReleaseRequestModel
////    {
////        public string BuildLbl { get; set; }
////        public string Branch { get; set; }
////        public string WorkItems { get; set; }
////        public string ContactEmail { get; set; }
////        public string TargetEnv { get; set; }
////        public string NonCodeRelease { get; set; }
////    }
////}

//////using Microsoft.AspNetCore.Mvc;


//////namespace WebApplication1.Controllers
//////{
//////    public class ReleaseRequestController : Controller
//////    {
//////        [HttpGet]
//////        public ActionResult Index()
//////        {
//////            ViewBag.FinalSubmission = "NOERROR"; // Replace with actual logic
//////            ViewBag.BuildLbl = "Build Label"; // Replace with actual logic
//////            ViewBag.Branch = "Branch"; // Replace with actual logic
//////            ViewBag.CloneTickets = ""; // Replace with actual logic
//////            ViewBag.Title = "Release Request"; // Replace with actual logic
//////            ViewBag.NonCodeRelease = "NO"; // Replace with actual logic

//////            return View();
//////        }

//////        [HttpPost]
//////        public ActionResult Submit(ReleaseRequestModel model)
//////        {
//////            // Handle form submission logic here
//////            return RedirectToAction("Index");
//////        }
//////    }

//////    public class ReleaseRequestModel
//////    {
//////        public string BuildLbl { get; set; }
//////        public string Branch { get; set; }
//////        public string WorkItems { get; set; }
//////        public string ContactEmail { get; set; }
//////        public string TargetEnv { get; set; }
//////        public string NonCodeRelease { get; set; }
//////    }
//////}




