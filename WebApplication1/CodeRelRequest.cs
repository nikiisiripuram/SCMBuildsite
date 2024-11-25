using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

public class Build
{
    public string Specific { get; set; }
    public DateTime DateAdded { get; set; }
    public string BranchFloating { get; set; }
    public string Label { get; set; }
    public string BuildOldStyle { get; set; }
    public int IID { get; set; }  // New field for RequesterEmail
    public string Email { get; set; }   
}

public class BuildRepository
{
    private readonly string _connectionString;

    public BuildRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public class DMCRRequestContext
    {
        private readonly string _connectionString;
        public DMCRRequestContext(string connectionString)
        {
            _connectionString = connectionString;
        }
    }

    public class ScmDbContext
    {
        private readonly string _connectionString;
        public ScmDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }
    }

    public List<Build> GetEligibleBuilds(string branch = null)
    {
        var builds = new List<Build>();
        string sql = @"
    DECLARE @dteMonthAgo DATETIME;
    SET @dteMonthAgo = DATEADD(MONTH, -1, GETDATE());

    SELECT 
        br.Specifics, 
        b.IID, 
        MAX(b.DateAdded) AS DateAdded,  -- Most recent DateAdded per branch
        b.BranchFloating, 
        b.Label, 
        b.BuildOldStyle, 
        db.Email AS Email
    FROM 
        dbo.tblBuild b WITH (NOLOCK)
        LEFT OUTER JOIN tblBuildEnvironment be WITH (NOLOCK)
            ON b.IID = be.IID
        LEFT OUTER JOIN dbo.tblBuildRequest br WITH (NOLOCK)
            ON br.BuildID = b.IID
        LEFT OUTER JOIN dbo.TblDevBuild db WITH (NOLOCK)
            ON db.Label = b.Label
    WHERE 
        be.IID IS NULL
        AND b.BuildActive = 1
        AND b.BuildRoot IS NOT NULL
        AND b.nonCode = 0
        AND b.DateAdded > @dteMonthAgo
        AND (@branch IS NULL OR br.Specifics LIKE '%' + @branch + '%')
    GROUP BY 
        br.Specifics, 
        b.IID, 
        b.BranchFloating, 
        b.Label, 
        b.BuildOldStyle, 
        db.Email
    ORDER BY 
        MAX(b.DateAdded) DESC,  -- Order by most recent DateAdded (descending)
        b.BranchFloating;";

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@branch", (object)branch ?? DBNull.Value);

            connection.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var build = new Build
                    {
                        Specific = reader["Specifics"].ToString(),
                        IID = Convert.ToInt32(reader["IID"]),
                        DateAdded = Convert.ToDateTime(reader["DateAdded"]),  // Use the updated column alias
                        BranchFloating = reader["BranchFloating"].ToString(),
                        Label = reader["Label"].ToString(),
                        BuildOldStyle = reader["BuildOldStyle"].ToString(),
                        Email = reader["Email"].ToString()  // Populate DevBuildEmail
                    };
                    builds.Add(build);
                }
            }
        }

        return builds;
    }


    public List<string> GetLabelsByEmail(string email)
    {
        var labels = new List<string>();
        string sql = @"
                          DECLARE @dteMonthAgo DATETIME;
SET @dteMonthAgo = DATEADD(MONTH, -1, GETDATE());

SELECT DISTINCT Label
FROM dbo.tblDevBuild WITH (NOLOCK)
WHERE Email = @Email
  AND Active = 1
  AND Label LIKE '%dev%'
ORDER BY Label DESC;
";


        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@Email", email);

            conn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    labels.Add(reader["Label"].ToString());
                }
            }
        }
        return labels;
    }


}








//public List<Build> GetBranchAndBuildLabels()
//{
//    var builds = new List<Build>();
//    string sql = @"
//        DECLARE @dteMonthAgo DATETIME;
//        SET @dteMonthAgo = DATEADD(MONTH, -1, GETDATE());

//        SELECT 
//            b.BranchFloating,
//            b.Label
//        FROM 
//            dbo.tblBuild b WITH (NOLOCK)
//            LEFT OUTER JOIN tblBuildEnvironment be WITH (NOLOCK)
//                ON b.IID = be.IID
//            LEFT OUTER JOIN dbo.tblBuildRequest br WITH (NOLOCK)
//                ON br.BuildID = b.IID
//        WHERE 
//            be.IID IS NULL
//            AND b.BuildActive = 1
//            AND b.BuildRoot IS NOT NULL
//            AND b.nonCode = 0
//            AND b.DateAdded > @dteMonthAgo
//        ORDER BY 
//            b.BranchFloating, 
//            b.DateAdded";

//    using (var conn = new SqlConnection(_connectionString))
//    using (var cmd = new SqlCommand(sql, conn))
//    {
//        conn.Open();
//        using (var reader = cmd.ExecuteReader())
//        {
//            while (reader.Read())
//            {
//                var build = new Build
//                {
//                    BranchFloating = reader["BranchFloating"].ToString(),
//                    Label = reader["Label"].ToString()
//                };
//                builds.Add(build);
//            }
//        }
//    }
//    return builds;
//}
