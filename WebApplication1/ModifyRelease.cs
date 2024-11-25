
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Dapper;

namespace WebApplication1
{
    public class BuildInfo
    {
        public string BuildOldStyle { get; set; }
        public string BranchFloating { get; set; }
        public string BuildNewStyle { get; set; }
        public string Label { get; set; }
        public string Project { get; set; }
        public string ConfigChanges { get; set; }
        public string SpecialInstructions { get; set; }
        public string NonCode { get; set; }
        public string DBScripts { get; set; }
    }

    public interface ISCMBuildHelper
    {
        void ScriptsInEnvs(string[] aScripts, string[] aEnvs);
        bool InEnv(string environment, string dbScript);
        string UnpackDBScripts(string sScripts);
        string GetDBScripts(string buildOldStyle, string tickets);
        string GetBuildDescriptionTable(int IID, bool includeEmail, bool compareScripts);
        string GetTix(string buildOldStyle);
        string GetBuildEmailList(int IID);
        Dictionary<string, string> GetContactINFO(int IID);
        string ClassifyDBScripts(string dbScripts);
        bool InactivateBuild(int IID);
    }

    public class SCMBuildHelper : ISCMBuildHelper
    {
        private readonly IDbConnection _scmConnection;
        private readonly IDbConnection _tdConnection;
        private readonly Dictionary<string, Dictionary<string, bool>> _scriptsInEnvs = new Dictionary<string, Dictionary<string, bool>>();

        public SCMBuildHelper(IDbConnection scmConnection, IDbConnection tdConnection)
        {
            _scmConnection = scmConnection;
            _tdConnection = tdConnection;
        }
        public bool InactivateBuild(int IID)
        {
            try
            {
                // SELECT statement to verify the record exists
                string selectSql = "SELECT * FROM dbo.tblBuild WHERE IID = @IID";
                var buildInfo = _tdConnection.QueryFirstOrDefault(selectSql, new { IID });

                if (buildInfo == null)
                {
                    return false; // Record not found
                }

                // UPDATE statement to inactivate the build
                string updateSql = "UPDATE dbo.tblBuild SET BuildActive = 0 WHERE IID = @IID";
                _scmConnection.Execute(updateSql, new { IID });

                return true; // Successful update
            }
            catch (Exception ex)
            {
                // Optionally log the exception
                Console.WriteLine($"Error inactivating build: {ex.Message}");
                return false;
            }
        }
        public void ScriptsInEnvs(string[] aScripts, string[] aEnvs)
        {
            for (int i = 0; i < aScripts.Length; i++)
            {
                if (aScripts[i].StartsWith("x"))
                {
                    aScripts[i] = aScripts[i].Substring(1);
                }
            }

            string sql = @"
                SELECT se.ScriptName, e.EnvName 
                FROM dbo.tbl_dB_Script_Environments se 
                INNER JOIN tbl_Environments e ON se.EnvID = e.EnvID 
                WHERE se.ScriptName IN @Scripts";

            if (aEnvs != null)
            {
                sql += " AND e.EnvName IN @Envs";
            }

            var results = _scmConnection.Query<(string ScriptName, string EnvName)>(sql, new { Scripts = aScripts, Envs = aEnvs });

            foreach (var result in results)
            {
                string scriptName = result.ScriptName.ToLower();
                string envName = result.EnvName.ToLower();

                if (!_scriptsInEnvs.ContainsKey(scriptName))
                {
                    _scriptsInEnvs[scriptName] = new Dictionary<string, bool>();
                }

                _scriptsInEnvs[scriptName][envName] = true;
            }

            foreach (string script in aScripts)
            {
                string scriptLower = script.ToLower();
                if (!_scriptsInEnvs.ContainsKey(scriptLower))
                {
                    _scriptsInEnvs[scriptLower] = new Dictionary<string, bool>();
                }
            }
        }

        public bool InEnv(string environment, string dbScript)
        {
            if (dbScript.StartsWith("x"))
            {
                dbScript = dbScript.Substring(1);
            }

            string scriptLower = dbScript.ToLower();

            if (!_scriptsInEnvs.ContainsKey(scriptLower))
            {
                ScriptsInEnvs(new[] { dbScript }, null);
            }

            if (_scriptsInEnvs.TryGetValue(scriptLower, out var envs))
            {
                return envs.ContainsKey(environment.ToLower());
            }

            return false;
        }

        public string UnpackDBScripts(string sScripts)
        {
            if (!string.IsNullOrEmpty(sScripts))
            {
                return sScripts.Replace("Ex_", "Express_");
            }
            return sScripts;
        }

        public string GetDBScripts(string buildOldStyle, string tickets)
        {
            var conditions = new List<string>();

            if (!string.IsNullOrWhiteSpace(buildOldStyle))
            {
                conditions.Add("bg_user_17 = @BuildOldStyle");
            }

            if (!string.IsNullOrWhiteSpace(tickets))
            {
                conditions.Add("bg_bug_id IN (@Tickets)");
            }

            if (conditions.Count == 0)
            {
                throw new ArgumentException("Internal error: No conditions specified.");
            }

            string sql = $"SELECT bg_user_18 FROM td.bug WHERE (NOT bg_user_18 = '') AND ({string.Join(" OR ", conditions)})";

            var scripts = _tdConnection.Query<string>(sql, new { BuildOldStyle = buildOldStyle, Tickets = tickets.Split(',').Select(s => s.Trim()) }).ToList();

            var finalDBScripts = new StringBuilder();
            foreach (var script in scripts)
            {
                if (script.Trim().StartsWith("x"))
                {
                    finalDBScripts.Append($"x{script.Trim().Substring(1)}, ");
                }
                else
                {
                    finalDBScripts.Append($"{script.Trim()}, ");
                }
            }

            return finalDBScripts.ToString().TrimEnd(',', ' ');
        }

        public string GetBuildDescriptionTable(int IID, bool includeEmail, bool compareScripts)
        {
            string sql = @"
                SELECT * FROM dbo.tblBuild b 
                LEFT JOIN tblBuildDBScripts s ON b.IID = s.IID 
                WHERE b.IID = @IID";

            var buildInfo = _scmConnection.QuerySingleOrDefault<BuildInfo>(sql, new { IID });

            if (buildInfo == null)
            {
                return string.Empty;
            }

            var sHtml = new StringBuilder("<table border=\"1\" bordercolor=\"#b3b3b3\" bgcolor=\"#d3d3d3\">");

            if (includeEmail)
            {
                sHtml.Append($"<tr><td>Distribution List<td>{GetBuildEmailList(IID)}");
            }

            var contactInfo = GetContactINFO(IID);
            bool bStarted = false;

            foreach (var contact in contactInfo.Values)
            {
                if (!string.IsNullOrWhiteSpace(contact))
                {
                    if (bStarted)
                    {
                        sHtml.Append("<tr><td>");
                    }
                    else
                    {
                        sHtml.Append("<tr><td>Contact Info.");
                        bStarted = true;
                    }
                    sHtml.Append($"<td>{contact}");
                }
            }

            sHtml.Append($"<tr><td>Branch (label)<td>{buildInfo.BranchFloating} ({buildInfo.Label})");
            sHtml.Append($"<tr><td>Build<td>{buildInfo.BuildOldStyle}");

            string sTickets = GetTix(buildInfo.BuildOldStyle);

            sHtml.Append($"<tr><td>Tickets<td>{sTickets}");

            var aAllScripts = GetDBScripts(buildInfo.BuildOldStyle, sTickets).Split(new[] { ", " }, StringSplitOptions.None);
            string[] aOldScripts = UnpackDBScripts(buildInfo.DBScripts).Split(new[] { ", " }, StringSplitOptions.None);

            if (compareScripts)
            {
                var aNewScripts = aAllScripts.Except(aOldScripts).ToArray();
                var aMissingScripts = aOldScripts.Except(aAllScripts).ToArray();
                var aBothScripts = aOldScripts.Intersect(aAllScripts).ToArray();

                sHtml.Append($"<tr><td>DB Scripts<td>{ClassifyDBScripts(string.Join(", ", aBothScripts))}");
                if (aNewScripts.Length > 0)
                {
                    sHtml.Append($"<tr><td>New DB Scripts<td>{ClassifyDBScripts(string.Join(", ", aNewScripts))}");
                }
                if (aMissingScripts.Length > 0)
                {
                    sHtml.Append($"<tr><td>Missing DB Scripts<td>{ClassifyDBScripts(string.Join(", ", aMissingScripts))}");
                }
            }
            else
            {
                sHtml.Append($"<tr><td>DB Scripts<td>{ClassifyDBScripts(string.Join(", ", aAllScripts))}");
            }

            if (!string.IsNullOrWhiteSpace(buildInfo.ConfigChanges))
            {
                sHtml.Append($"<tr><td>Configuration Changes<td><pre>{buildInfo.ConfigChanges}</pre>");
            }

            if (!string.IsNullOrWhiteSpace(buildInfo.SpecialInstructions))
            {
                sHtml.Append($"<tr><td>Special Instructions<td><pre>{buildInfo.SpecialInstructions}</pre>");
            }

            sHtml.Append("</table>");

            return sHtml.ToString();
        }

        public string GetTix(string buildOldStyle)
        {
            string sql = "SELECT bg_bug_id FROM td.bug WHERE bg_user_17 = @BuildOldStyle ORDER BY bg_bug_id";
            var tickets = _tdConnection.Query<string>(sql, new { BuildOldStyle = buildOldStyle }).ToList();
            return string.Join(", ", tickets);
        }

        public string GetBuildEmailList(int IID)
        {
            string sql = @"
                SELECT x.EmailList 
                FROM dbo.tblProjectVersionXref x 
                INNER JOIN tblBuild b ON b.BranchFloating LIKE x.Project + '-' + x.Version + '-%' 
                WHERE b.IID = @IID";

            var emailLists = _scmConnection.Query<string>(sql, new { IID });
            return string.Join("; ", emailLists.Select(AddDomainToAddresses));
        }

        public Dictionary<string, string> GetContactINFO(int IID)
        {
            string sql = @"
        SELECT p.UserName, p.Development, p.QA, p.Management, p.Primary, p.Secondary 
        FROM dbo.tblBuildContactXref x 
        LEFT JOIN tblProjectContact p ON x.UserName = p.UserName 
        WHERE x.IID = @IID";

            var contactInfo = new Dictionary<string, string>();

            var results = _scmConnection.Query(sql, new { IID });

            foreach (var row in results)
            {
                // Cast the dynamic key to string
                string key = (string)row.UserName;
                string value = $"{row.Development}, {row.QA}, {row.Management}, {row.Primary}, {row.Secondary}";

                contactInfo[key] = value;
            }

            return contactInfo;
        }

        public string ClassifyDBScripts(string dbScripts)
        {
            string result = string.Empty;
            const string delimiter = ", ";

            var arrDBScripts = dbScripts.Replace(" ", "").Split(',');

            var envs = new[] { "Prod", "QA1", "QA2", "QA3" };
            ScriptsInEnvs(arrDBScripts, envs);

            foreach (var script in arrDBScripts)
            {
                if (script.Contains("_hld_"))
                {
                    result += delimiter + $"<font color=\"green\"><u><b>{script}</b></u></font>";
                }
                else if (InEnv("Prod", script))
                {
                    result += delimiter + $"<strike>{script}</strike>";
                }
                else if (InEnv("QA1", script))
                {
                    result += delimiter + $"<font color=\"red\">{script}</font>";
                }
                else if (InEnv("QA2", script))
                {
                    result += delimiter + $"<font color=\"red\"><i>{script}</i></font>";
                }
                else if (InEnv("QA3", script))
                {
                    result += delimiter + $"<font color=\"blue\">{script}</font>";
                }
                else
                {
                    result += delimiter + script;
                }
            }

            return result.TrimStart(delimiter.ToCharArray());
        }

        private string AddDomainToAddresses(string addressList)
        {
            var addressArray = addressList.Split(';').Select(addr => addr.Trim());

            var sb = new StringBuilder();
            foreach (var address in addressArray)
            {
                if (!string.IsNullOrEmpty(address) && !address.Contains("@"))
                {
                    sb.Append($"{address}@yourdomain.com; ");
                }
                else
                {
                    sb.Append($"{address}; ");
                }
            }

            return sb.ToString().TrimEnd(' ', ';');
        }
    }

}
