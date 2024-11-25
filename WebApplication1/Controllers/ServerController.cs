//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Linq;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;
//using WebApplication1.Models;

//namespace WebApplication1.Controllers
//{
//    public class ServerController : Controller
//    {
//        private readonly IConfiguration _configuration;

//        public ServerController(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }

//        public ActionResult Server(string sortExpression = null, string envFilter = "ALL", string appFilter = "ALL")
//        {
//            ViewBag.EnvFilter = envFilter;
//            ViewBag.AppFilter = appFilter;
//            ViewBag.SortDirection = ViewBag.SortDirection == null ? "ASC" : ViewBag.SortDirection.ToString();

//            var servers = GetServers(envFilter, appFilter);

//            if (!string.IsNullOrEmpty(sortExpression))
//            {
//                servers = sortExpression switch
//                {
//                    "Application" => ViewBag.SortDirection == "ASC" ? servers.OrderBy(s => s.Application).ToList() : servers.OrderByDescending(s => s.Application).ToList(),
//                    "Environment" => ViewBag.SortDirection == "ASC" ? servers.OrderBy(s => s.Environment).ToList() : servers.OrderByDescending(s => s.Environment).ToList(),
//                    _ => servers
//                };
//                ViewBag.SortDirection = ViewBag.SortDirection == "ASC" ? "DESC" : "ASC";
//            }

//            return View(servers);
//        }

//        private List<ServerInfo> GetServers(string envFilter, string appFilter)
//        {
//            var environments = new List<string> { "dev4", "qa", "Staging", "Prod", "EU-Stg", "EU-Prod" };
//            var applications = new List<string> { "CompanionProducts", "Phoenix-SSIS", "Phoenix", "DST", "Enterprise", "LM", "Reporting", "GTG", "Workbench", "Search", "PSE", "Dashboard", "SCM" };

//            envFilter = envFilter.ToLower() == "all" ? string.Join(",", environments.Select(e => $"'{e}'")) : $"'{envFilter}'";
//            appFilter = appFilter.ToLower() == "all" ? string.Join(",", applications.Select(a => $"'{a}'")) : $"'{appFilter}'";

//            var constr = _configuration.GetConnectionString("ScmDefaultConnection");
//            var strSQL = $@"
//                SELECT DISTINCT UPPER(s.Server) Server, i.serverip, e.Environment, a.Application 
//                FROM tblServer s 
//                INNER JOIN tblServerEnvironmentApplicationXref x ON s.ServerID = x.ServerID
//                INNER JOIN tblServerIP i ON s.ServerID = i.ServerID
//                INNER JOIN tblEnvironment e ON x.EnvironmentID = e.EnvironmentID
//                INNER JOIN tblApplication a ON x.ApplicationID = a.ApplicationID 
//                WHERE s.ServerActive = 1 
//                AND e.Environment IN ({envFilter}) 
//                AND a.Application IN ({appFilter}) 
//                ORDER BY serverip DESC";

//            var servers = new List<ServerInfo>();

//            using (var con = new SqlConnection(constr))
//            {
//                using (var cmd = new SqlCommand(strSQL, con))
//                {
//                    con.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            servers.Add(new ServerInfo
//                            {
//                                Server = reader["Server"].ToString(),
//                                ServerIP = reader["serverip"].ToString(),
//                                Environment = reader["Environment"].ToString(),
//                                Application = reader["Application"].ToString()
//                            });
//                        }
//                    }
//                }
//            }

//            return servers;
//        }
//    }
//}

//using System.Collections.Generic;
//using System.Data;
//using System.Data.SqlClient;
//using System.Linq;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;
//using WebApplication1.Models;

//namespace WebApplication1.Controllers
//{
//    public class ServerController : Controller
//    {
//        private readonly IConfiguration _configuration;

//        public ServerController(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }

//        public ActionResult Server(string sortExpression = null, string envFilter = "ALL", string appFilter = "ALL")
//        {
//            ViewBag.EnvFilter = envFilter;
//            ViewBag.AppFilter = appFilter;
//            ViewBag.SortDirection = ViewBag.SortDirection == null ? "ASC" : ViewBag.SortDirection.ToString();

//            var servers = GetServers(envFilter, appFilter);

//            if (!string.IsNullOrEmpty(sortExpression))
//            {
//                servers = sortExpression switch
//                {
//                    "Application" => ViewBag.SortDirection == "ASC" ? servers.OrderBy(s => s.Application).ToList() : servers.OrderByDescending(s => s.Application).ToList(),
//                    "Environment" => ViewBag.SortDirection == "ASC" ? servers.OrderBy(s => s.Environment).ToList() : servers.OrderByDescending(s => s.Environment).ToList(),
//                    _ => servers
//                };
//                ViewBag.SortDirection = ViewBag.SortDirection == "ASC" ? "DESC" : "ASC";
//            }

//            return View(servers);
//        }

//        private List<ServerInfo> GetServers(string envFilter, string appFilter)
//        {
//            var environments = new List<string> { "dev4", "qa", "Staging", "Prod", "EU-Stg", "EU-Prod" };
//            var applications = new List<string> { "CompanionProducts", "Phoenix-SSIS", "Phoenix", "DST", "Enterprise", "LM", "Reporting", "GTG", "Workbench", "Search", "PSE", "Dashboard", "SCM" };

//            envFilter = envFilter.ToLower() == "all" ? string.Join(",", environments.Select(e => $"'{e}'")) : $"'{envFilter}'";
//            appFilter = appFilter.ToLower() == "all" ? string.Join(",", applications.Select(a => $"'{a}'")) : $"'{appFilter}'";

//            var constr = _configuration.GetConnectionString("ScmDefaultConnection");
//            var strSQL = $@"
//                SELECT DISTINCT UPPER(s.Server) Server, i.serverip, e.Environment, a.Application 
//                FROM tblServer s 
//                INNER JOIN tblServerEnvironmentApplicationXref x ON s.ServerID = x.ServerID
//                INNER JOIN tblServerIP i ON s.ServerID = i.ServerID
//                INNER JOIN tblEnvironment e ON x.EnvironmentID = e.EnvironmentID
//                INNER JOIN tblApplication a ON x.ApplicationID = a.ApplicationID 
//                WHERE s.ServerActive = 1 
//                AND e.Environment IN ({envFilter}) 
//                AND a.Application IN ({appFilter}) 
//                ORDER BY serverip DESC";

//            var servers = new List<ServerInfo>();

//            using (var con = new SqlConnection(constr))
//            {
//                using (var cmd = new SqlCommand(strSQL, con))
//                {
//                    con.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            servers.Add(new ServerInfo
//                            {
//                                Server = reader["Server"].ToString(),
//                                ServerIP = reader["serverip"].ToString(),
//                                Environment = reader["Environment"].ToString(),
//                                Application = reader["Application"].ToString()
//                            });
//                        }
//                    }
//                }
//            }

//            return servers;
//        }
//    }
//}


//using System.Collections.Generic;
//using System.Data;
//using System.Data.SqlClient;
//using System.Linq;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;
//using WebApplication1.Models;

//namespace WebApplication1.Controllers
//{
//    public class ServerController : Controller
//    {
//        private readonly IConfiguration _configuration;

//        public ServerController(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }

//        public ActionResult Server(string sortExpression = null, string envFilter = "ALL", string appFilter = "ALL", string sortDirection = "ASC")
//        {
//            ViewBag.EnvFilter = envFilter;
//            ViewBag.AppFilter = appFilter;
//            ViewBag.SortExpression = sortExpression;
//            ViewBag.SortDirection = sortDirection;

//            var servers = GetServers(envFilter, appFilter);

//            if (!string.IsNullOrEmpty(sortExpression))
//            {
//                servers = sortExpression switch
//                {
//                    "Application" => sortDirection == "ASC" ? servers.OrderBy(s => s.Application).ToList() : servers.OrderByDescending(s => s.Application).ToList(),
//                    "Environment" => sortDirection == "ASC" ? servers.OrderBy(s => s.Environment).ToList() : servers.OrderByDescending(s => s.Environment).ToList(),
//                    "Server" => sortDirection == "ASC" ? servers.OrderBy(s => s.Server).ToList() : servers.OrderByDescending(s => s.Server).ToList(),
//                    "ServerIP" => sortDirection == "ASC" ? servers.OrderBy(s => s.ServerIP).ToList() : servers.OrderByDescending(s => s.ServerIP).ToList(),
//                    _ => servers
//                };
//            }

//            // Returning the entire view if it is a normal request
//            if (!Request.IsAjaxRequest())
//            {
//                return View(servers);
//            }

//            // Returning partial view if it is an AJAX request
//            return PartialView("_Server", servers);
//        }

//        private List<ServerInfo> GetServers(string envFilter, string appFilter)
//        {
//            var environments = new List<string> { "dev4", "qa", "Staging", "Prod", "EU-Stg", "EU-Prod" };
//            var applications = new List<string> { "CompanionProducts", "Phoenix-SSIS", "Phoenix", "DST", "Enterprise", "LM", "Reporting", "GTG", "Workbench", "Search", "PSE", "Dashboard", "SCM" };

//            envFilter = envFilter.ToLower() == "all" ? string.Join(",", environments.Select(e => $"'{e}'")) : $"'{envFilter}'";
//            appFilter = appFilter.ToLower() == "all" ? string.Join(",", applications.Select(a => $"'{a}'")) : $"'{appFilter}'";

//            var constr = _configuration.GetConnectionString("ScmDefaultConnection");
//            var strSQL = $@"
//                SELECT DISTINCT UPPER(s.Server) Server, i.serverip, e.Environment, a.Application 
//                FROM tblServer s 
//                INNER JOIN tblServerEnvironmentApplicationXref x ON s.ServerID = x.ServerID
//                INNER JOIN tblServerIP i ON s.ServerID = i.ServerID
//                INNER JOIN tblEnvironment e ON x.EnvironmentID = e.EnvironmentID
//                INNER JOIN tblApplication a ON x.ApplicationID = a.ApplicationID 
//                WHERE s.ServerActive = 1 
//                AND e.Environment IN ({envFilter}) 
//                AND a.Application IN ({appFilter}) 
//                ORDER BY serverip DESC";

//            var servers = new List<ServerInfo>();

//            using (var con = new SqlConnection(constr))
//            {
//                using (var cmd = new SqlCommand(strSQL, con))
//                {
//                    con.Open();
//                    using (var reader = cmd.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            servers.Add(new ServerInfo
//                            {
//                                Server = reader["Server"].ToString(),
//                                ServerIP = reader["serverip"].ToString(),
//                                Environment = reader["Environment"].ToString(),
//                                Application = reader["Application"].ToString()
//                            });
//                        }
//                    }
//                }
//            }

//            return servers;
//        }
//    }

//    public static class HttpRequestExtensions
//    {
//        public static bool IsAjaxRequest(this Microsoft.AspNetCore.Http.HttpRequest request)
//        {
//            if (request == null)
//            {
//                throw new ArgumentNullException(nameof(request));
//            }

//            if (request.Headers != null)
//            {
//                return request.Headers["X-Requested-With"] == "XMLHttpRequest";
//            }

//            return false;
//        }
//    }
//}


using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ServerController : Controller
    {
        private readonly IConfiguration _configuration;

        public ServerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Server(string envFilter = "ALL", string appFilter = "ALL")
        {
            List<ServerInfo> servers = FetchServers(envFilter, appFilter);
            return View(servers);
        }

        private List<ServerInfo> FetchServers(string envFilter, string appFilter)
        {
            var environments = new List<string> { "dev4", "qa", "Staging", "Prod", "EU-Stg", "EU-Prod" };
            var applications = new List<string> { "CompanionProducts", "Phoenix-SSIS", "Phoenix", "DST", "Enterprise", "LM", "Reporting", "GTG", "Workbench", "Search", "PSE", "Dashboard", "SCM" };

            envFilter = envFilter.ToLower() == "all" ? string.Join(",", environments.Select(e => $"'{e}'")) : $"'{envFilter}'";
            appFilter = appFilter.ToLower() == "all" ? string.Join(",", applications.Select(a => $"'{a}'")) : $"'{appFilter}'";

            var connectionString = _configuration.GetConnectionString("ScmDefaultConnection");
            var query = $@"
                SELECT DISTINCT UPPER(s.Server) AS Server, i.serverip, e.Environment, a.Application 
                FROM tblServer s 
                INNER JOIN tblServerEnvironmentApplicationXref x ON s.ServerID = x.ServerID
                INNER JOIN tblServerIP i ON s.ServerID = i.ServerID
                INNER JOIN tblEnvironment e ON x.EnvironmentID = e.EnvironmentID
                INNER JOIN tblApplication a ON x.ApplicationID = a.ApplicationID 
                WHERE s.ServerActive = 1 
                AND e.Environment IN ({envFilter}) 
                AND a.Application IN ({appFilter}) 
                ORDER BY i.serverip DESC";

            var servers = new List<ServerInfo>();

            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            servers.Add(new ServerInfo
                            {
                                Server = reader["Server"].ToString(),
                                ServerIP = reader["serverip"].ToString(),
                                Environment = reader["Environment"].ToString(),
                                Application = reader["Application"].ToString()
                            });
                        }
                    }
                }
            }

            return servers;
        }
    }
}
