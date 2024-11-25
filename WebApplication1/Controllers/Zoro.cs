
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using WebApplication1.Models;

[Authorize(Policy = "WindowsPolicy")]
public class ZoroController : Controller
{
    private readonly string _connectionString;

    public ZoroController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public IActionResult Builds(FilterViewModel model)
    {
        // Fetch the builds based on the filtering criteria.
        var builds = GetBuilds(model);
        model.Builds = PrepareBuildViewModels(builds);

        return View(model);
    }

    [HttpPost]
    public IActionResult Filter(FilterViewModel model)
    {
        // Redirect to the Builds action with the filtering criteria.
        return RedirectToAction("Builds", model);
    }

    private List<BuildModel> GetBuilds(FilterViewModel model)
    {
        var builds = new List<BuildModel>();

        using (SqlConnection cn = new SqlConnection(_connectionString))
        {
            // Construct the SQL query based on the filtering criteria.
            string sql = @"SELECT DISTINCT b.IID, b.BuildOldStyle, b.BuildNewStyle, b.Label, b.DateAdded, b.BuildRoot, 
                                  b.SpecialInstructions, b.ConfigChanges
                           FROM tblBuild b WITH (NOLOCK)
                           LEFT JOIN td.BUG d WITH (NOLOCK) ON d.BG_USER_17 = b.BuildOldStyle
                           LEFT JOIN tblBuildEnvironment e WITH (NOLOCK) ON e.IID = b.IID";

            if (model.Tickets)
            {
                sql += " INNER JOIN td.BUG g WITH (NOLOCK) ON g.BG_USER_17 = b.BuildOldStyle";
            }
            if (model.ProdOnly)
            {
                sql += " INNER JOIN dbo.tblBuildEnvironment e2 WITH (NOLOCK) ON e2.IID = b.IID AND e2.EnvId = 7";
            }

            sql += " WHERE b.BuildRoot IS NOT NULL AND b.DateAdded > GETDATE() - 65";

            if (!string.IsNullOrEmpty(model.BuildPattern))
            {
                sql += " AND (b.Project LIKE @BuildPattern OR b.BuildOldStyle LIKE @BuildPattern OR b.BuildNewStyle LIKE @BuildPattern)";
            }

            if (!model.ShowInactive && !model.ProdOnly)
            {
                sql += " AND b.BuildActive = 1";
            }
            if (!model.ShowUnreleased)
            {
                sql += " AND LEN(b.RequesterEmail) > 0";
            }
            if (model.ProdOnly)
            {
                sql += " AND e.EnvId = 7 AND b.BuildActive = 0";
            }

            sql += " ORDER BY b.BuildRoot, b.DateAdded DESC";

            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                if (!string.IsNullOrEmpty(model.BuildPattern))
                {
                    cmd.Parameters.AddWithValue("@BuildPattern", "%" + model.BuildPattern.Replace("'", "''") + "%");
                }

                cn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                // Populate the builds list.
                while (reader.Read())
                {
                    builds.Add(new BuildModel
                    {
                        Id = Convert.ToInt32(reader["IID"]),
                        BuildOldStyle = reader["BuildOldStyle"].ToString(),
                        BuildNewStyle = reader["BuildNewStyle"].ToString(),
                        Label = reader["Label"].ToString(),
                        DateAdded = Convert.ToDateTime(reader["DateAdded"]),
                        SpecialInstructions = reader["SpecialInstructions"].ToString(),
                        ConfigChanges = reader["ConfigChanges"].ToString()
                    });
                }
            }
        }

        return builds;
    }

    private List<BuildModel> PrepareBuildViewModels(List<BuildModel> builds)
    {
        var buildViewModels = new List<BuildModel>();
        var buildIDs = builds.Select(b => b.Id).ToList();
        var buildEnvironments = ReGetBuildsEnvironments(buildIDs);

        foreach (var build in builds)
        {
            var viewModel = new BuildModel
            {
                Id = build.Id,
                BuildOldStyle = build.BuildOldStyle,
                BuildNewStyle = build.BuildNewStyle,
                Label = build.Label,
                DateAdded = build.DateAdded,
                EnvAbbreviations = new string[4] // Initialize the array with 4 empty slots for CQA, CSTG, PR, EU
            };

            // Map environments to the view model.
            if (buildEnvironments.ContainsKey(build.Id.ToString()))
            {
                var envIds = buildEnvironments[build.Id.ToString()];

                foreach (var envId in envIds)
                {
                    var envAbbr = GetEnvironmentAbbreviation(envId);
                    switch (envAbbr)
                    {
                        case "CQA":
                            viewModel.EnvAbbreviations[0] = "CQA";
                            break;
                        case "CStg":
                            viewModel.EnvAbbreviations[1] = "CStg";
                            break;
                        case "PR":
                            viewModel.EnvAbbreviations[2] = "PR";
                            break;
                        case "EU":
                            viewModel.EnvAbbreviations[3] = "EU";
                            break;
                    }
                }
            }

            // Check for Special Instructions or Config Changes and add "SI" link if necessary
            viewModel.HasSpecialInstructionsOrConfigChanges = !string.IsNullOrEmpty(build.SpecialInstructions) || !string.IsNullOrEmpty(build.ConfigChanges);
            if (viewModel.HasSpecialInstructionsOrConfigChanges)
            {
                viewModel.SILink = GenerateLink(build.Id, "SI", "ModifyRelease_Step1.asp");
            }

            buildViewModels.Add(viewModel);
        }

        return buildViewModels;
    }

    private Dictionary<string, List<int>> ReGetBuildsEnvironments(List<int> aIIDs)
    {
        var oScmLibBuildEnvironments = new Dictionary<string, List<int>>();

        using (SqlConnection mycn = new SqlConnection(_connectionString))
        {
            string sSql = "SELECT IID, EnvId FROM tblBuildEnvironment ";
            if (aIIDs != null && aIIDs.Count > 0)
            {
                sSql += "WHERE IID IN (" + string.Join(",", aIIDs) + ") ";
            }
            sSql += "ORDER BY IID";

            using (SqlCommand cmd = new SqlCommand(sSql, mycn))
            {
                mycn.Open();
                using (SqlDataReader myrs = cmd.ExecuteReader())
                {
                    List<int> aIds = new List<int>();
                    int? nIID = null;

                    while (myrs.Read())
                    {
                        int currentIID = Convert.ToInt32(myrs["IID"]);

                        if (nIID != currentIID)
                        {
                            if (aIds.Count > 0)
                            {
                                oScmLibBuildEnvironments[nIID.ToString()] = new List<int>(aIds);
                                aIds.Clear();
                            }
                            nIID = currentIID;
                        }
                        aIds.Add(Convert.ToInt32(myrs["EnvId"]));
                    }

                    if (aIds.Count > 0 && nIID != null)
                    {
                        oScmLibBuildEnvironments[nIID.ToString()] = new List<int>(aIds);
                    }
                }
            }
        }

        return oScmLibBuildEnvironments;
    }

    private string GetEnvironmentAbbreviation(int envId)
    {
        string abbreviation = string.Empty;

        using (SqlConnection cn = new SqlConnection(_connectionString))
        {
            string sql = "SELECT EnvAbbreviation FROM dbo.tbl_Environments WHERE EnvId = @EnvId";

            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@EnvId", envId);

                cn.Open();
                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    abbreviation = result.ToString();
                }
            }
        }

        return abbreviation;
    }

    // Helper method to generate a link
    private string GenerateLink(int id, string action, string url)
    {
        return $"<a href='/{url}?IID={id}'>{action}</a>";
    }
}
