using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace WebApplication1.Services
{
    public class EnvironmentService
    {
        private List<Environment> environments;
        private Dictionary<int, int> envIdIndex;
        private Dictionary<string, int> envNameIndex;

        private readonly string _connectionString;

        public EnvironmentService(IConfiguration configuration)
        {
            environments = new List<Environment>();
            envIdIndex = new Dictionary<int, int>();
            envNameIndex = new Dictionary<string, int>();
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public string[] GetEnvironmentNames()
        {
            if (environments.Count == 0)
            {
                LoadEnvironments();
            }

            string[] envNames = new string[environments.Count];
            for (int i = 0; i < environments.Count; i++)
            {
                envNames[i] = environments[i].EnvName;
            }

            return envNames;
        }

        private void LoadEnvironments()
        {
            string query = "SELECT EnvId, EnvName, Version, EnvDescription, EnvAbbreviation, SortOrder " +
                           "FROM dbo.tbl_Environments WHERE Active=1 ORDER BY SortOrder";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Environment env = new Environment
                    {
                        EnvId = Convert.ToInt32(reader["EnvId"]),
                        EnvName = reader["EnvName"].ToString().Trim(),
                        Version = reader["Version"].ToString().Trim(),
                        EnvDescription = reader["EnvDescription"].ToString().Trim(),
                        EnvAbbreviation = reader["EnvAbbreviation"].ToString().Trim(),
                        SortOrder = Convert.ToInt32(reader["SortOrder"])
                    };

                    environments.Add(env);
                    envIdIndex[env.EnvId] = environments.Count - 1;
                    envNameIndex[env.EnvName] = environments.Count - 1;
                }

                reader.Close();
            }
        }

        private class Environment
        {
            public int EnvId { get; set; }
            public string EnvName { get; set; }
            public string Version { get; set; }
            public string EnvDescription { get; set; }
            public string EnvAbbreviation { get; set; }
            public int SortOrder { get; set; }
        }
    }
}
