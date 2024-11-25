//using System;
//using System.Text;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Data.SqlClient;
//using System.Net.Mail;
//using AzureClientLibrary;
//using WebApplication1.Models;
//using System.Web;
//using System.Collections;
//using WebApplication1.Controllers;
//using WebApplication1.Models;

//namespace WebApplication1.Services
////{
//    public interface IReleaseRequestService
//    {
//        BuildDetails GetBuildDetails(string IID);
//        string ReleaseRequestSubmit(ReleaseRequestViewModel model);
//        ArrayList ValidateWorkItems(string workItems);
//    }

//    public class ReleaseRequestService : IReleaseRequestService
//    {
//        public BuildDetails GetBuildDetails(string IID)
//        {
//            var buildDetails = new BuildDetails();
//            //string conStr = ConfigurationManager.ConnectionStrings["tdBrassringQAConnectionString"].ConnectionString;
//            string strSQL = "Select * from tblBuild where IID=" + IID;

//            using (SqlConnection con = new SqlConnection(conStr))
//            {
//                con.Open();
//                using (SqlCommand cmd = new SqlCommand(strSQL, con))
//                {
//                    SqlDataReader dr = cmd.ExecuteReader();
//                    if (dr.Read())
//                    {
//                        buildDetails.BuildLabel = dr["BuildNewStyle"].ToString();
//                        buildDetails.Branch = dr["Branch"].ToString();
//                    }
//                }
//                con.Close();
//            }

//            return buildDetails;
//        }

//        public string ReleaseRequestSubmit(ReleaseRequestViewModel model)
//        {
//            BoardsWorkItems boardsWIObj = new BoardsWorkItems(ConfigurationManager.AppSettings["AZProjectName"], ConfigurationManager.AppSettings["PAT"]);
//            TblBuildDBScriptsUpdate(model);

//            model.WorkItems = model.WorkItems.Replace(" ", "");
//            string finalsubmit = boardsWIObj.updateTicketsInReleaseRequest(model.WorkItems, model.BuildLabel);
//            if (finalsubmit.Equals("NOERROR"))
//            {
//                SendReleaseMail(model);
//            }
//            return finalsubmit;
//        }

//        private void TblBuildDBScriptsUpdate(ReleaseRequestViewModel model)
//        {
//            string strSQL;
//            string conStr = ConfigurationManager.ConnectionStrings["tdBrassringQAConnectionString"].ConnectionString;
//            string currentTime = DateTime.Now.ToString();
//            string targetEnv2 = model.TargetEnv.Replace(" Now", "").Replace(" Upon QA Approval", "").Replace("EU-Prod", "Prod").Replace("US-Prod", "Prod").Replace("One-Stg", "Prod");
//            string NCIID = "";
//            string tempSI1 = "";

//            using (SqlConnection con = new SqlConnection(conStr))
//            {
//                con.Open();
//                try
//                {
//                    if (model.NonCodeRelease.Equals("YES"))
//                    {
//                        string krbPrefix = model.NCReleaseProduct.Contains("KRBUtilities") ? "" : "KRB";

//                        if (targetEnv2.Contains(","))
//                        {
//                            targetEnv2 = targetEnv2.Split(',')[0];
//                        }
//                        model.Branch = model.NCReleaseProduct + "-" + targetEnv2;

//                        strSQL = "Insert into tblBuild (Project, Branch, BranchFloating, Label, DateAdded, BuildActive, RequesterEmail, EmailList, ConfigChanges, SpecialInstructions, nonCode, DateReleaseRequested) " +
//                                 "Values ('" + krbPrefix + model.NCReleaseProduct.Replace("-", "") + "', '" + currentTime + "', '" + model.NCReleaseProduct + "-" + targetEnv2 + "', '" + currentTime + "', '" + currentTime + "', 1, '" + model.ContactEmail + "', '" + model.ContactEmail + "', '', '" + tempSI1 + "', 1, '" + currentTime + "')";
//                        using (SqlCommand cmd = new SqlCommand(strSQL, con))
//                        {
//                            cmd.ExecuteNonQuery();
//                        }

//                        strSQL = "SELECT IID, BuildOldStyle FROM dbo.tblBuild  WITH (nolock) WHERE Project = '" + krbPrefix + model.NCReleaseProduct.Replace("-", "") + "' AND Branch = '" + currentTime + "' AND BranchFloating = '" + model.NCReleaseProduct + "-" + targetEnv2 + "'";

//                        using (SqlCommand cmd = new SqlCommand(strSQL, con))
//                        {
//                            SqlDataReader dr1 = cmd.ExecuteReader();
//                            if (dr1.Read())
//                            {
//                                model.BuildLabel = dr1["BuildOldStyle"].ToString();
//                                NCIID = dr1["IID"].ToString();
//                                model.BuildLabel = model.BuildLabel.Replace(".", " NC ");
//                            }
//                        }
//                    }
//                    else
//                    {
//                        strSQL = "Update tblBuild Set " +
//                                 "RequesterEmail='" + model.ContactEmail + "', " +
//                                 "EmailList='" + model.ContactEmail + "', " +
//                                 "ConfigChanges='" + "" + "', " +
//                                 "SpecialInstructions='" + tempSI1 + "', " +
//                                 "DateReleaseRequested='" + currentTime + "' " +
//                                 "Where IID=" + model.IID;
//                        using (SqlCommand cmd = new SqlCommand(strSQL, con))
//                        {
//                            cmd.ExecuteNonQuery();
//                        }

//                        strSQL = "SELECT IID FROM tblBuildDBScripts WITH (nolock) WHERE IID=" + model.IID;
//                        SqlDataReader dr2 = null;
//                        using (SqlCommand cmd = new SqlCommand(strSQL, con))
//                        {
//                            dr2 = cmd.ExecuteReader();
//                        }

//                        if (dr2.Read())
//                        {
//                            strSQL = "UPDATE tblBuildDBScripts SET DBScripts='" + model.WorkItemsDBScripts + "' WHERE IID=" + model.IID;
//                        }
//                        else
//                        {
//                            strSQL = "INSERT INTO tblBuildDBScripts (IID, DBScripts) VALUES (" + model.IID + ", '" + model.WorkItemsDBScripts + "')";
//                        }
//                        using (SqlCommand cmd = new SqlCommand(strSQL, con))
//                        {
//                            cmd.ExecuteNonQuery();
//                        }
//                    }
//                    con.Close();
//                }
//                catch (Exception Ex)
//                {
//                    SendErrorEmail(Ex, model);
//                }
//            }
//        }

//        private void SendReleaseMail(ReleaseRequestModel model)
//        {
//            StringBuilder EmailBody = new StringBuilder();
//            EmailBody.Append("<table border=1 bordercolor=gray bgcolor=#d3d3d3>");
//            EmailBody.Append("<tr><td>Contact-Info</td><td>" + model.ContactEmail + "</td></tr>");
//            EmailBody.Append("<tr><td>Target Env</td><td>" + model.TargetEnv + "</td></tr>");
//            EmailBody.Append("<tr><td>Build Label</td><td>" + model.BuildLabel + "</td></tr>");
//            EmailBody.Append("<tr><td>Branch</td><td>" + model.Branch + "</td></tr>");
//            EmailBody.Append("<tr><td>DB Script WorkItems</td><td>" + model.WorkItemsDBScripts + "</td></tr>");
//            EmailBody.Append("<tr><td>Special Instructions</td><td>" + model.WorkItemsSI + "</td></tr>");
//            EmailBody.Append("</table>");
//            string EmailSubject = string.IsNullOrEmpty(model.BuildLabel) ? "New Non-Code Release Request" : "New Code Release Request";
//            EmailSubject += " - " + DateTime.Now;

//            MailMessage MailMsg = new MailMessage
//            {
//                From = new MailAddress(model.ContactEmail),
//                Subject = EmailSubject,
//                Body = EmailBody.ToString(),
//                IsBodyHtml = true
//            };

//            string ToEmail = ConfigurationManager.AppSettings["mailList"];
//            foreach (var toAddress in ToEmail.Split(';'))
//            {
//                MailMsg.To.Add(toAddress);
//            }

//            SmtpClient SmtpClient = new SmtpClient
//            {
//                Host = ConfigurationManager.AppSettings["mailRelayServer"],
//                Port = 25
//            };

//            SmtpClient.Send(MailMsg);
//        }

//        private void SendErrorEmail(Exception Ex, ReleaseRequestModel model)
//        {
//            StringBuilder ErrorEmailBody = new StringBuilder();
//            ErrorEmailBody.Append("<p>Exception occured when inserting non-code release details.</p>");
//            ErrorEmailBody.Append("<p>Exception Details:</p>");
//            ErrorEmailBody.Append("<table border=1 bordercolor=gray bgcolor=#d3d3d3>");
//            ErrorEmailBody.Append("<tr><td>Exception</td><td>" + Ex.ToString() + "</td></tr>");
//            ErrorEmailBody.Append("<tr><td>Contact-Info</td><td>" + model.ContactEmail + "</td></tr>");
//            ErrorEmailBody.Append("<tr><td>Target Env</td><td>" + model.TargetEnv + "</td></tr>");
//            ErrorEmailBody.Append("<tr><td>Build Label</td><td>" + model.BuildLabel + "</td></tr>");
//            ErrorEmailBody.Append("<tr><td>Branch</td><td>" + model.Branch + "</td></tr>");
//            ErrorEmailBody.Append("<tr><td>DB Script WorkItems</td><td>" + model.WorkItemsDBScripts + "</td></tr>");
//            ErrorEmailBody.Append("<tr><td>Special Instructions</td><td>" + model.WorkItemsSI + "</td></tr>");
//            ErrorEmailBody.Append("</table>");
//            string ErrorEmailSubject = "Error occurred - " + DateTime.Now;

//            MailMessage ErrorMailMsg = new MailMessage
//            {
//                From = new MailAddress(model.ContactEmail),
//                Subject = ErrorEmailSubject,
//                Body = ErrorEmailBody.ToString(),
//                IsBodyHtml = true
//            };

//            string ErrorToEmail = ConfigurationManager.AppSettings["mailList"];
//            foreach (var toAddress in ErrorToEmail.Split(';'))
//            {
//                ErrorMailMsg.To.Add(toAddress);
//            }

//            SmtpClient SmtpClient = new SmtpClient
//            {
//                Host = ConfigurationManager.AppSettings["mailRelayServer"],
//                Port = 25
//            };

//            SmtpClient.Send(ErrorMailMsg);
//        }

//        public ArrayList ValidateWorkItems(string workItems)
//        {
//            ArrayList workItemValidation = new ArrayList();
//            string[] workItemsArr = workItems.Replace(" ", "").Split(',');

//            foreach (string workItem in workItemsArr)
//            {
//                if (!workItem.StartsWith("NC"))
//                {
//                    workItemValidation.Add(workItem);
//                }
//            }

//            return workItemValidation;
//        }
//    }
//}
