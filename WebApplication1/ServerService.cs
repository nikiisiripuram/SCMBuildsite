//using System;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Linq;
//using Microsoft.Extensions.Configuration;
//using WebApplication1.Models;

//namespace WebApplication1.Services
//{
//    public class ServerService
//    {
//        private readonly IConfiguration _configuration;

//        public ServerService(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }

//        public List<ServerInfo> FetchServers(string envFilter, string appFilter)
//        {
//            var environments = new List<string> { "Dev4", "QA", "Staging", "US-Prod", "EU-Stg", "EU-Prod" };
//            var applications = new List<string> { "CompanionProducts", "Phoenix-SSIS", "Phoenix", "DST", "Enterprise", "LM", "Reporting", "GTG", "Workbench", "Search",  "Dashboard", "SCM" };

//            envFilter = envFilter.ToLower() == "all" ? string.Join(",", environments.Select(e => $"'{e}'")) : $"'{envFilter}'";
//            appFilter = appFilter.ToLower() == "all" ? string.Join(",", applications.Select(a => $"'{a}'")) : $"'{appFilter}'";

//            var connectionString = _configuration.GetConnectionString("ScmDefaultConnection");
//            var query = $@"
//                SELECT DISTINCT UPPER(s.Server) AS Server, i.serverip, e.Environment, a.Application 
//                FROM tblServer s 
//                INNER JOIN tblServerEnvironmentApplicationXref x ON s.ServerID = x.ServerID
//                INNER JOIN tblServerIP i ON s.ServerID = i.ServerID
//                INNER JOIN tblEnvironment e ON x.EnvironmentID = e.EnvironmentID
//                INNER JOIN tblApplication a ON x.ApplicationID = a.ApplicationID 
//                WHERE s.ServerActive = 1 
//                AND e.Environment IN ({envFilter}) 
//                AND a.Application IN ({appFilter}) 
//                ORDER BY i.serverip DESC";

//            var servers = new List<ServerInfo>();

//            using (var connection = new SqlConnection(connectionString))
//            {
//                using (var command = new SqlCommand(query, connection))
//                {
//                    connection.Open();
//                    using (var reader = command.ExecuteReader())
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


using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class ServerService
    {
        private readonly IConfiguration _configuration;

        public ServerService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<ServerInfo> FetchServers(string envFilter = "all", string appFilter = "all")
        {
            // Define the allowed environments and applications
            var environments = new List<string> { "Dev4", "QA", "Staging", "Prod", "EU-Stg", "EU-Prod" };
            var applications = new List<string> { "CompanionProducts", "Phoenix-SSIS", "Phoenix", "DST", "Enterprise", "LM", "Reporting", "GTG", "Workbench", "Search", "PSE", "Dashboard", "SCM" };

            // Handle the filter logic
            envFilter = envFilter.ToLower() == "all" ? string.Join(",", environments.Select(e => $"'{e}'")) : $"'{envFilter}'";
            appFilter = appFilter.ToLower() == "all" ? string.Join(",", applications.Select(a => $"'{a}'")) : $"'{appFilter}'";

            var connectionString = _configuration.GetConnectionString("ScmDefaultConnection");

            var query = $@"
                SELECT DISTINCT 
                    UPPER(s.Server) AS Server, 
                    i.serverip, 
                    e.Environment, 
                    a.Application 
                FROM 
                    tblServer s 
                    INNER JOIN tblServerEnvironmentApplicationXref x 
                        ON s.ServerID = x.ServerID 
                    INNER JOIN tblServerIP i 
                        ON s.ServerID = i.ServerID 
                    INNER JOIN tblEnvironment e 
                        ON x.EnvironmentID = e.EnvironmentID 
                    INNER JOIN tblApplication a 
                        ON x.ApplicationID = a.ApplicationID 
                WHERE 
                    s.ServerActive = 1 
                    AND e.Environment IN ({envFilter}) 
                    AND a.Application IN ({appFilter}) 
                ORDER BY 
                    i.serverip DESC";

            var servers = new List<ServerInfo>();

            // Execute the SQL query and map the results to a list of ServerInfo objects
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
