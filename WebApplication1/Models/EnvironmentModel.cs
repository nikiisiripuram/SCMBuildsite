namespace WebApplication1.Models
{
    public class EnvironmentModel
    {
        public int EnvId { get; set; }              // Unique ID for the environment
        public string EnvName { get; set; }         // Name of the environment (e.g., "CQA", "PR")
        public string EnvAbbreviation { get; set; } // Abbreviation of the environment (e.g., "QA", "PRD")
        public bool IsActive { get; set; }          // Whether the environment is active or not
        public int SortOrder { get; set; }          // Sort order for displaying environments
    }
}