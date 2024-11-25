
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using WebApplication1.Models;
using Microsoft.AspNetCore.Authorization;  
namespace WebApplication1.Controllers
{
    [Authorize(Policy = "WindowsPolicy")]
    public class SanjiController : Controller
    {
        private readonly IDbConnection _scmConnection;
        private readonly IDbConnection _tdConnection;
        private readonly ISCMBuildHelper _scmBuildHelper;

        public SanjiController(IDbConnection scmConnection, IDbConnection tdConnection, ISCMBuildHelper scmBuildHelper)
        {
            _scmConnection = scmConnection;
            _tdConnection = tdConnection;
            _scmBuildHelper = scmBuildHelper;
        }

        public IActionResult ModifyRelease(int IID)
        {
            var buildInfo = GetBuildInfo(IID);
            ViewData["BuildInfo"] = buildInfo;
            return View();
        }

        private BuildInfo GetBuildInfo(int IID)
        {
            var buildInfo = new BuildInfo();

            using (var cn = _tdConnection)
            {
                cn.Open();
                string sql = @"SELECT b.BuildOldStyle, b.BranchFloating, b.BuildNewStyle, 
                                b.Label, b.Project, b.ConfigChanges, 
                                b.SpecialInstructions, b.nonCode, s.DBScripts 
                               FROM dbo.tblBuild b 
                               LEFT JOIN tblBuildDBScripts s ON b.IID = s.IID 
                               WHERE b.IID = @IID";

                using (var cmd = new SqlCommand(sql, (SqlConnection)cn))
                {
                    cmd.Parameters.AddWithValue("@IID", IID);   

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            buildInfo.BuildOldStyle = reader["BuildOldStyle"]?.ToString();
                            buildInfo.BranchFloating = reader["BranchFloating"]?.ToString();
                            buildInfo.BuildNewStyle = reader["BuildNewStyle"]?.ToString();
                            buildInfo.Label = reader["Label"]?.ToString();
                            buildInfo.Project = reader["Project"]?.ToString();
                            buildInfo.ConfigChanges = reader["ConfigChanges"]?.ToString();
                            buildInfo.SpecialInstructions = reader["SpecialInstructions"]?.ToString();
                            buildInfo.NonCode = reader["nonCode"]?.ToString();
                            buildInfo.DBScripts = reader["DBScripts"]?.ToString();
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(buildInfo.SpecialInstructions))
            {
                buildInfo.SpecialInstructions = buildInfo.SpecialInstructions
                    .Replace("<br>", "\n")
                    .Replace("&lt;br&gt;", "\n")
                    .Replace("<b>", "")
                    .Replace("&lt;b&gt;", "")
                    .Replace("</b>", "")
                    .Replace("&lt;/b&gt;", "")
                    .Replace("</br>", "")
                    .Replace("&lt;/br&gt;", "");
            }


            return buildInfo;
        }

        [HttpPost]
        public IActionResult SendEmail(EmailViewModel model)
        {
            // Construct the email body
            var emailBody = $"<p align=\"center\">{_scmBuildHelper.GetBuildDescriptionTable(model.IID, includeEmail: false, compareScripts: true)}</p>";

            // If EmailList is null or empty, get the email list from the build
            if (string.IsNullOrWhiteSpace(model.EmailList))
            {
                model.EmailList = _scmBuildHelper.GetBuildEmailList(model.IID);
            }

            // Modify EmailList based on script flags
            if (!model.HadScripts)
            {
                model.EmailList = model.EmailList.Replace("dba@brassring.com", "").Replace(";;", ";").Replace("; ;", ";");
            }

            // Initialize the script link
            string strScriptLink = string.Empty;
            if (model.HadScripts && model.HasScripts)
            {
                var link = $"https://buildsite/DBATools/ScriptDeploy_EnvSelect.asp?IID={model.IID.ToString().Replace(" ", "+")}";
                strScriptLink = $"DBA's: When requested from SCM to roll out scripts for this release, <a href=\"{link}\">Click Here</a> to browse to the DB script roll out tool.<br><br>";
            }

            // Set the email subject
            string subject = model.NonCode ? $"NON-CODE RELEASE for {model.Build}" : $"RELEASE for {model.Build}";

            // Send the email
            using (var smtpClient = new SmtpClient("smtp.yourserver.com"))
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("walthamconfigurationmanagement@brassring.com"),
                    Subject = subject,
                    Body = strScriptLink + emailBody,
                    IsBodyHtml = true
                };

                foreach (var email in model.EmailList.Split(new[] { "; " }, StringSplitOptions.RemoveEmptyEntries))
                {
                    mailMessage.To.Add(email);
                }

                smtpClient.Send(mailMessage);
            }

            // Redirect to the original URL
            return Redirect(model.URLFROM);
        }


        [HttpPost]
        public IActionResult InactivateBuild(int IID)
        {
            // Call the service method to inactivate the build
            bool success = _scmBuildHelper.InactivateBuild(IID);

            // Prepare the ViewData to pass to the ModifyRelease view
            ViewData["Success"] = success;
            ViewData["Message"] = success
                ? "Build inactivated successfully!"
                : "Build not found or an error occurred.";

            // Fetch the updated build info to display in the ModifyRelease view
            var buildInfo = _scmBuildHelper.GetBuildDescriptionTable(IID, includeEmail: false, compareScripts: true);

            // Return the ModifyRelease view with the updated build information
            return View("ModifyRelease", buildInfo);
        }
    }
}



//using System;
//using System.Data;
//using Microsoft.Data.SqlClient;
//using Dapper;
//using Microsoft.AspNetCore.Mvc;
//using System.Net.Mail;
//using WebApplication1.Models;

//namespace WebApplication1.Controllers
//{
//    public class SanjiController : Controller
//    {
//        private readonly IDbConnection _scmConnection;
//        private readonly IDbConnection _tdConnection;
//        private readonly ISCMBuildHelper _scmBuildHelper;

//        public SanjiController(IDbConnection scmConnection, IDbConnection tdConnection, ISCMBuildHelper scmBuildHelper)
//        {
//            _scmConnection = scmConnection;
//            _tdConnection = tdConnection;
//            _scmBuildHelper = scmBuildHelper;
//        }

//        public IActionResult ModifyRelease(int IID)
//        {
//            var buildInfo = GetBuildInfo(IID);
//            if (buildInfo == null)
//            {
//                // Handle the case where buildInfo is not found
//                return NotFound("Build information not found.");
//            }

//            // Pass both BuildInfo and an initialized EmailViewModel to the view
//            var model = new Tuple<BuildInfo, EmailViewModel>(buildInfo, new EmailViewModel { IID = IID });
//            return View(model);
//        }

//        private BuildInfo GetBuildInfo(int IID)
//        {
//            const string sql = @"
//                SELECT b.BuildOldStyle, b.BranchFloating, b.BuildNewStyle, 
//                       b.Label, b.Project, b.ConfigChanges, 
//                       b.SpecialInstructions, b.nonCode, s.DBScripts 
//                FROM dbo.tblBuild b 
//                LEFT JOIN tblBuildDBScripts s ON b.IID = s.IID 
//                WHERE b.IID = @IID";

//            using (var cn = _tdConnection)
//            {
//                cn.Open();
//                var buildInfo = cn.QueryFirstOrDefault<BuildInfo>(sql, new { IID });

//                if (buildInfo != null && !string.IsNullOrEmpty(buildInfo.SpecialInstructions))
//                {
//                    buildInfo.SpecialInstructions = CleanSpecialInstructions(buildInfo.SpecialInstructions);
//                }

//                return buildInfo;
//            }
//        }

//        private string CleanSpecialInstructions(string instructions)
//        {
//            return instructions
//                .Replace("<br>", "\n")
//                .Replace("&lt;br&gt;", "\n")
//                .Replace("<b>", "")
//                .Replace("&lt;b&gt;", "")
//                .Replace("</b>", "")
//                .Replace("&lt;/b&gt;", "")
//                .Replace("</br>", "")
//                .Replace("&lt;/br&gt;", "");
//        }

//        [HttpPost]
//        public IActionResult SendEmail(EmailViewModel model)
//        {
//            if (model == null) return BadRequest("Invalid email model.");

//            // Get or build the email list
//            model.EmailList = string.IsNullOrWhiteSpace(model.EmailList)
//                ? _scmBuildHelper.GetBuildEmailList(model.IID)
//                : model.EmailList;

//            // Remove DBA emails if there are no scripts
//            if (!model.HadScripts)
//            {
//                model.EmailList = model.EmailList.Replace("dba@brassring.com", "").Replace(";;", ";").Replace("; ;", ";");
//            }

//            // Construct the email body
//            var emailBody = ConstructEmailBody(model);

//            // Set the email subject
//            string subject = model.NonCode ? $"NON-CODE RELEASE for {model.Build}" : $"RELEASE for {model.Build}";

//            // Send the email
//            SendEmail(model.EmailList, subject, emailBody);

//            return Redirect(model.URLFROM);
//        }

//        private string ConstructEmailBody(EmailViewModel model)
//        {
//            var emailBody = $"<p align=\"center\">{_scmBuildHelper.GetBuildDescriptionTable(model.IID, includeEmail: false, compareScripts: true)}</p>";
//            if (model.HadScripts && model.HasScripts)
//            {
//                var link = $"https://buildsite/DBATools/ScriptDeploy_EnvSelect.asp?IID={model.IID.ToString().Replace(" ", "+")}";
//                emailBody = $"DBA's: When requested from SCM to roll out scripts for this release, <a href=\"{link}\">Click Here</a> to browse to the DB script roll out tool.<br><br>" + emailBody;
//            }
//            return emailBody;
//        }

//        private void SendEmail(string emailList, string subject, string body)
//        {
//            using (var smtpClient = new SmtpClient("smtp.yourserver.com"))
//            {
//                var mailMessage = new MailMessage
//                {
//                    From = new MailAddress("walthamconfigurationmanagement@brassring.com"),
//                    Subject = subject,
//                    Body = body,
//                    IsBodyHtml = true
//                };

//                foreach (var email in emailList.Split(new[] { "; " }, StringSplitOptions.RemoveEmptyEntries))
//                {
//                    mailMessage.To.Add(email);
//                }

//                smtpClient.Send(mailMessage);
//            }
//        }

//        [HttpPost]
//        public IActionResult InactivateBuild(int IID)
//        {
//            bool success = _scmBuildHelper.InactivateBuild(IID);

//            ViewData["Success"] = success;
//            ViewData["Message"] = success
//                ? "Build inactivated successfully!"
//                : "Build not found or an error occurred.";

//            var buildInfo = GetBuildInfo(IID);

//            if (buildInfo == null)
//            {
//                return NotFound("Build information not found.");
//            }

//            var model = new Tuple<BuildInfo, EmailViewModel>(buildInfo, new EmailViewModel { IID = IID });

//            return View("ModifyRelease", model);
//        }
//    }
//}
