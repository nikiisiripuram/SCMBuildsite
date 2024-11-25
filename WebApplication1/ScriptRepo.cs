//using Microsoft.Data.SqlClient;
//using System;
//using System.Collections.Generic;
//using WebApplication1.Models;

//namespace WebApplication1.Repositories
//{
//    public class ScriptRepository
//    {
//        private readonly string _connectionString;

//        public ScriptRepository(string connectionString)
//        {
//            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
//        }

//        public ScriptDetail GetScriptDetail()
//        {
//            var scriptDetail = new ScriptDetail();
//            string scriptName = "OperationalDB_dbc_11.00.1893.001";
//            using (var connection = new SqlConnection(_connectionString))
//            {
//                connection.Open();

//                // Retrieve main script details
//                var scriptCommand = new SqlCommand(
//                    "SELECT s.ScriptName, s.DateRequested, s.Requestor, s.DatabaseSchema, s.AppVersion, s.EntitiesAffected, s.Comments, s.BuildID, se.MaxEnvID " +
//                    "FROM dbo.tbl_dB_Scripts s WITH(NOLOCK) " +
//                    "LEFT OUTER JOIN (" +
//                    "    SELECT EnvSort.EnvId as MaxEnvId, EnvSort.ScriptName " +
//                    "    FROM (" +
//                    "        SELECT ta1.EnvId, ta1.ScriptName, ta2.SortOrder, ta2.EnvName " +
//                    "        FROM dbo.tbl_DB_Script_Environments ta1 (NOLOCK), tbl_Environments ta2 " +
//                    "        WHERE ta1.EnvId = ta2.EnvId" +
//                    "    ) as EnvSort " +
//                    "    WHERE sortOrder in (" +
//                    "        SELECT MAX(SortOrder) " +
//                    "        FROM (" +
//                    "            SELECT tb1.EnvId, tb1.ScriptName, tb2.SortOrder, tb2.EnvName " +
//                    "            FROM dbo.tbl_DB_Script_Environments tb1 (NOLOCK), tbl_Environments tb2 " +
//                    "            WHERE tb1.EnvId = tb2.EnvId " +
//                    "              AND EnvSort.scriptname = ScriptName" +
//                    "        ) AS a" +
//                    "    )" +
//                    ") se ON s.ScriptName = se.ScriptName " +
//                    "WHERE s.ScriptName = @ScriptName", connection);
//                scriptCommand.Parameters.AddWithValue("@ScriptName", scriptName);

//                using (var reader = scriptCommand.ExecuteReader())
//                {
//                    if (reader.Read())
//                    {
//                        scriptDetail.ScriptName = reader["ScriptName"].ToString();
//                        scriptDetail.DateRequested = Convert.ToDateTime(reader["DateRequested"]);
//                        scriptDetail.Requestor = reader["Requestor"].ToString();
//                        scriptDetail.DatabaseSchema = reader["DatabaseSchema"].ToString();
//                        scriptDetail.Application = reader["AppVersion"].ToString();
//                        scriptDetail.EntitiesAffected = reader["EntitiesAffected"].ToString();
//                        scriptDetail.Comments = reader["Comments"].ToString();
//                        scriptDetail.BuildID = reader["BuildID"].ToString();
//                        scriptDetail.MaxEnvID = reader["MaxEnvID"].ToString();
//                    }
//                }

//                // Retrieve tickets
//                var ticketCommand = new SqlCommand(
//                    "SELECT t.TicketID, t.Comments " +
//                    "FROM dbo.tbl_dB_Scripts s WITH(NOLOCK) " +
//                    "INNER JOIN dbo.tbl_dB_Script_Tickets t WITH(NOLOCK) ON s.ScriptName = t.ScriptName " +
//                    "WHERE s.ScriptName = @ScriptName", connection);
//                ticketCommand.Parameters.AddWithValue("@ScriptName", scriptName);

//                using (var reader = ticketCommand.ExecuteReader())
//                {
//                    scriptDetail.Tickets = new List<ScriptTicket>();
//                    while (reader.Read())
//                    {
//                        var ticket = new ScriptTicket
//                        {
//                            TicketID = reader["TicketID"].ToString(),
//                            Comments = reader["Comments"].ToString()
//                        };
//                        scriptDetail.Tickets.Add(ticket);
//                    }
//                }

//                // Retrieve environments
//                var envCommand = new SqlCommand(
//                    "SELECT e.EnvID, e.EnvName, se.DateApplied " +
//                    "FROM dbo.tbl_dB_Scripts s WITH(NOLOCK) " +
//                    "INNER JOIN dbo.tbl_dB_Script_Environments se WITH(NOLOCK) ON s.ScriptName = se.ScriptName " +
//                    "INNER JOIN dbo.tbl_Environments e WITH(NOLOCK) ON se.EnvID = e.EnvID " +
//                    "WHERE s.ScriptName = @ScriptName " +
//                    "ORDER BY e.SortOrder", connection);
//                envCommand.Parameters.AddWithValue("@ScriptName", scriptName);

//                using (var reader = envCommand.ExecuteReader())
//                {
//                    scriptDetail.Environments = new List<ScriptEnvironment>();
//                    while (reader.Read())
//                    {
//                        var environment = new ScriptEnvironment
//                        {
//                            EnvID = reader["EnvID"].ToString(),
//                            EnvName = reader["EnvName"].ToString(),
//                            DateApplied = Convert.ToDateTime(reader["DateApplied"])
//                        };
//                        scriptDetail.Environments.Add(environment);
//                    }
//                }
//            }
//            return scriptDetail;
//        }
//    }
//}

// File path: Repositories/ScriptRepository.cs


//using System;
//using System.Collections.Generic;
//using System.Data.SqlClient;

//namespace WebApplication1.Services
//{
//    public class ScriptService
//    {
//        private readonly string _connectionString;

//        public ScriptService(string connectionString)
//        {
//            _connectionString = connectionString;
//        }

//        public ScriptDetail GetScriptDetail(string scriptName)
//        {
//            ScriptDetail scriptDetail = new ScriptDetail();

//            using (SqlConnection conn = new SqlConnection(_connectionString))
//            {
//                conn.Open();

//                // Fetch script details
//                string scriptQuery = @"
//                    SELECT s.ScriptName, s.DateRequested, s.Requestor, s.DatabaseSchema, s.AppVersion, 
//                           s.EntitiesAffected, s.Comments, s.BuildID, se.MaxEnvID 
//                    FROM dbo.tbl_dB_Scripts s WITH(NOLOCK)
//                    LEFT OUTER JOIN (
//                        SELECT EnvSort.EnvId as MaxEnvId, EnvSort.ScriptName 
//                        FROM (
//                            SELECT ta1.EnvId, ta1.ScriptName, ta2.SortOrder, ta2.EnvName 
//                            FROM dbo.tbl_DB_Script_Environments ta1 (NOLOCK), tbl_Environments ta2 
//                            WHERE ta1.EnvId = ta2.EnvId
//                        ) as EnvSort 
//                        WHERE sortOrder in (
//                            SELECT MAX(SortOrder) 
//                            FROM (
//                                SELECT tb1.EnvId, tb1.ScriptName, tb2.SortOrder, tb2.EnvName 
//                                FROM dbo.tbl_DB_Script_Environments tb1 (NOLOCK), tbl_Environments tb2 
//                                WHERE tb1.EnvId = tb2.EnvId 
//                                  AND EnvSort.scriptname = ScriptName
//                            ) AS a
//                        )
//                    ) se ON s.ScriptName = se.ScriptName 
//                    WHERE s.ScriptName = @ScriptName";

//                SqlCommand cmd = new SqlCommand(scriptQuery, conn);
//                cmd.Parameters.AddWithValue("@ScriptName", scriptName);

//                using (SqlDataReader reader = cmd.ExecuteReader())
//                {
//                    if (reader.Read())
//                    {
//                        scriptDetail.ScriptName = reader["ScriptName"].ToString();
//                        scriptDetail.DateRequested = reader["DateRequested"] != DBNull.Value ? (DateTime?)reader["DateRequested"] : null;
//                        scriptDetail.Requestor = reader["Requestor"].ToString();
//                        scriptDetail.DatabaseSchema = reader["DatabaseSchema"].ToString();
//                        scriptDetail.AppVersion = reader["AppVersion"].ToString();
//                        scriptDetail.EntitiesAffected = reader["EntitiesAffected"].ToString();
//                        scriptDetail.Comments = reader["Comments"].ToString();
//                        scriptDetail.BuildID = reader["BuildID"].ToString();
//                        scriptDetail.MaxEnvID = reader["MaxEnvID"] != DBNull.Value ? (int?)reader["MaxEnvID"] : null;
//                    }
//                }

//                // Fetch ticket details
//                string ticketsQuery = @"
//                    SELECT t.TicketID, t.Comments 
//                    FROM dbo.tbl_dB_Scripts s WITH(NOLOCK)
//                    INNER JOIN dbo.tbl_dB_Script_Tickets t WITH(NOLOCK) ON s.ScriptName = t.ScriptName 
//                    WHERE s.ScriptName = @ScriptName";

//                cmd = new SqlCommand(ticketsQuery, conn);
//                cmd.Parameters.AddWithValue("@ScriptName", scriptName);

//                scriptDetail.Tickets = new List<Ticket>();
//                using (SqlDataReader reader = cmd.ExecuteReader())
//                {
//                    while (reader.Read())
//                    {
//                        scriptDetail.Tickets.Add(new Ticket
//                        {
//                            TicketID = reader["TicketID"].ToString(),
//                            Comments = reader["Comments"].ToString()
//                        });
//                    }
//                }

//                // Fetch environment details
//                string envsQuery = @"
//                    SELECT e.EnvID, e.EnvName, se.DateApplied 
//                    FROM dbo.tbl_dB_Scripts s WITH(NOLOCK)
//                    INNER JOIN dbo.tbl_dB_Script_Environments se WITH(NOLOCK) ON s.ScriptName = se.ScriptName 
//                    INNER JOIN dbo.tbl_Environments e WITH(NOLOCK) ON se.EnvID = e.EnvID 
//                    WHERE s.ScriptName = @ScriptName";

//                cmd = new SqlCommand(envsQuery, conn);
//                cmd.Parameters.AddWithValue("@ScriptName", scriptName);

//                scriptDetail.Environments = new List<EnvironmentDetail>();
//                using (SqlDataReader reader = cmd.ExecuteReader())
//                {
//                    while (reader.Read())
//                    {
//                        scriptDetail.Environments.Add(new EnvironmentDetail
//                        {
//                            EnvID = (int)reader["EnvID"],
//                            EnvName = reader["EnvName"].ToString(),
//                            DateApplied = reader["DateApplied"] != DBNull.Value ? (DateTime?)reader["DateApplied"] : null
//                        });
//                    }
//                }
//            }

//            return scriptDetail;
//        }
//    }

//    public class ScriptDetail
//    {
//        public string ScriptName { get; set; }
//        public DateTime? DateRequested { get; set; }
//        public string Requestor { get; set; }
//        public string DatabaseSchema { get; set; }
//        public string AppVersion { get; set; }
//        public string EntitiesAffected { get; set; }
//        public string Comments { get; set; }
//        public string BuildID { get; set; }
//        public int? MaxEnvID { get; set; }
//        public List<Ticket> Tickets { get; set; }
//        public List<EnvironmentDetail> Environments { get; set; }
//    }

//    public class Ticket
//    {
//        public string TicketID { get; set; }
//        public string Comments { get; set; }
//    }

//    public class EnvironmentDetail
//    {
//        public int EnvID { get; set; }
//        public string EnvName { get; set; }
//        public DateTime? DateApplied { get; set; }
//    }
//}


//working code//

//using System.Data;
//using System.Data.SqlClient;
//using Microsoft.Extensions.Configuration;

//public class ScriptRepository
//{
//    private readonly string _connectionString;

//    public ScriptRepository(IConfiguration configuration)
//    {
//        _connectionString = configuration.GetConnectionString("DefaultConnection");
//    }

//    public DataSet GetScriptsData()
//    {
//        using (SqlConnection connection = new SqlConnection(_connectionString))
//        {
//            string query = @"
//            SELECT s.ScriptName, s.DateRequested, s.Requestor, s.DatabaseSchema, s.AppVersion, 
//                   s.EntitiesAffected, s.Comments, s.BuildID, se.MaxEnvID 
//            FROM dbo.tbl_dB_Scripts s WITH(NOLOCK)
//            LEFT OUTER JOIN (
//                SELECT EnvSort.EnvId as MaxEnvId, EnvSort.ScriptName 
//                FROM (
//                    SELECT ta1.EnvId, ta1.ScriptName, ta2.SortOrder, ta2.EnvName 
//                    FROM dbo.tbl_DB_Script_Environments ta1 (NOLOCK), tbl_Environments ta2 
//                    WHERE ta1.EnvId = ta2.EnvId
//                ) as EnvSort 
//                WHERE sortOrder in (
//                    SELECT MAX(SortOrder) 
//                    FROM (
//                        SELECT tb1.EnvId, tb1.ScriptName, tb2.SortOrder, tb2.EnvName 
//                        FROM dbo.tbl_DB_Script_Environments tb1 (NOLOCK), tbl_Environments tb2 
//                        WHERE tb1.EnvId = tb2.EnvId 
//                          AND EnvSort.scriptname = ScriptName
//                    ) AS a
//                )
//            ) se ON s.ScriptName = se.ScriptName 
//            WHERE s.ScriptName = 'OperationalDB_dbc_11.00.1893.001';

//            SELECT t.TicketID, t.Comments 
//            FROM dbo.tbl_dB_Scripts s WITH(NOLOCK)
//            INNER JOIN dbo.tbl_dB_Script_Tickets t WITH(NOLOCK) ON s.ScriptName = t.ScriptName 
//            WHERE s.ScriptName = 'OperationalDB_dbc_11.00.1893.001';

//            SELECT e.EnvID, e.EnvName, se.DateApplied 
//            FROM dbo.tbl_dB_Scripts s WITH(NOLOCK)
//            INNER JOIN dbo.tbl_dB_Script_Environments se WITH(NOLOCK) ON s.ScriptName = se.ScriptName 
//            INNER JOIN dbo.tbl_Environments e WITH(NOLOCK) ON se.EnvID = e.EnvID 
//            WHERE s.ScriptName = 'OperationalDB_dbc_11.00.1893.001';
//        ";

//            SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
//            DataSet dataSet = new DataSet();
//            adapter.Fill(dataSet);

//            return dataSet;
//        }
//    }

//}


//using System.Data;
//using System.Data.SqlClient;
//using Microsoft.Extensions.Configuration;

//public class ScriptRepository
//{
//    private readonly string _connectionString;

//    public ScriptRepository(IConfiguration configuration)
//    {
//        _connectionString = configuration.GetConnectionString("DefaultConnection");
//    }

//    public DataSet GetScriptData(string scriptName)
//    {
//        using (SqlConnection connection = new SqlConnection(_connectionString))
//        {
//            string query = @"
//                SELECT s.ScriptName, s.DateRequested, s.Requestor, s.DatabaseSchema, s.AppVersion, 
//                       s.EntitiesAffected, s.Comments, s.BuildID, se.MaxEnvID 
//                FROM dbo.tbl_dB_Scripts s WITH(NOLOCK)
//                LEFT OUTER JOIN (
//                    SELECT EnvSort.EnvId as MaxEnvId, EnvSort.ScriptName 
//                    FROM (
//                        SELECT ta1.EnvId, ta1.ScriptName, ta2.SortOrder, ta2.EnvName 
//                        FROM dbo.tbl_DB_Script_Environments ta1 (NOLOCK), tbl_Environments ta2 
//                        WHERE ta1.EnvId = ta2.EnvId
//                    ) as EnvSort 
//                    WHERE sortOrder in (
//                        SELECT MAX(SortOrder) 
//                        FROM (
//                            SELECT tb1.EnvId, tb1.ScriptName, tb2.SortOrder, tb2.EnvName 
//                            FROM dbo.tbl_DB_Script_Environments tb1 (NOLOCK), tbl_Environments tb2 
//                            WHERE tb1.EnvId = tb2.EnvId 
//                              AND EnvSort.scriptname = ScriptName
//                        ) AS a
//                    )
//                ) se ON s.ScriptName = se.ScriptName 
//                WHERE s.ScriptName = @scriptName;

//                SELECT t.TicketID, t.Comments 
//                FROM dbo.tbl_dB_Scripts s WITH(NOLOCK)
//                INNER JOIN dbo.tbl_dB_Script_Tickets t WITH(NOLOCK) ON s.ScriptName = t.ScriptName 
//                WHERE s.ScriptName = @scriptName;

//                SELECT e.EnvID, e.EnvName, se.DateApplied 
//                FROM dbo.tbl_dB_Scripts s WITH(NOLOCK)
//                INNER JOIN dbo.tbl_dB_Script_Environments se WITH(NOLOCK) ON s.ScriptName = se.ScriptName 
//                INNER JOIN dbo.tbl_Environments e WITH(NOLOCK) ON se.EnvID = e.EnvID 
//                WHERE s.ScriptName = @scriptName;
//            ";

//            SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
//            adapter.SelectCommand.Parameters.AddWithValue("@scriptName", scriptName);
//            DataSet dataSet = new DataSet();
//            adapter.Fill(dataSet);

//            return dataSet;
//        }
//    }
//}


using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public class ScriptRepository
{
    private readonly string _connectionString;

    public ScriptRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public DataSet GetScriptData(string scriptName)
    {
        if (string.IsNullOrEmpty(scriptName))
        {
            throw new ArgumentException("scriptName cannot be null or empty.", nameof(scriptName));
        }

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"
                SELECT s.ScriptName, s.DateRequested, s.Requestor, s.DatabaseSchema, s.AppVersion, 
                       s.EntitiesAffected, s.Comments, s.BuildID, se.MaxEnvID 
                FROM dbo.tbl_dB_Scripts s WITH(NOLOCK)
                LEFT OUTER JOIN (
                    SELECT EnvSort.EnvId as MaxEnvId, EnvSort.ScriptName 
                    FROM (
                        SELECT ta1.EnvId, ta1.ScriptName, ta2.SortOrder, ta2.EnvName 
                        FROM dbo.tbl_DB_Script_Environments ta1 (NOLOCK), tbl_Environments ta2 
                        WHERE ta1.EnvId = ta2.EnvId
                    ) as EnvSort 
                    WHERE sortOrder in (
                        SELECT MAX(SortOrder) 
                        FROM (
                            SELECT tb1.EnvId, tb1.ScriptName, tb2.SortOrder, tb2.EnvName 
                            FROM dbo.tbl_DB_Script_Environments tb1 (NOLOCK), tbl_Environments tb2 
                            WHERE tb1.EnvId = tb2.EnvId 
                              AND EnvSort.scriptname = ScriptName
                        ) AS a
                    )
                ) se ON s.ScriptName = se.ScriptName 
                WHERE s.ScriptName = @scriptName;

                SELECT t.TicketID, t.Comments 
                FROM dbo.tbl_dB_Scripts s WITH(NOLOCK)
                INNER JOIN dbo.tbl_dB_Script_Tickets t WITH(NOLOCK) ON s.ScriptName = t.ScriptName 
                WHERE s.ScriptName = @scriptName;

                SELECT e.EnvID, e.EnvName, se.DateApplied 
                FROM dbo.tbl_dB_Scripts s WITH(NOLOCK)
                INNER JOIN dbo.tbl_dB_Script_Environments se WITH(NOLOCK) ON s.ScriptName = se.ScriptName 
                INNER JOIN dbo.tbl_Environments e WITH(NOLOCK) ON se.EnvID = e.EnvID 
                WHERE s.ScriptName = @scriptName;
            ";

            SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
            adapter.SelectCommand.Parameters.AddWithValue("@scriptName", scriptName);
            DataSet dataSet = new DataSet();
            adapter.Fill(dataSet);

            return dataSet;
        }
    }
}

