namespace WebApplication1.Models
{
    public class ReleaseRequestViewModel
    {
        public string BuildLabel { get; set; }
        public string Branch { get; set; }
        public string WorkItems { get; set; }
        public string Email { get; set; }
        public string TargetEnv { get; set; }
        public string FinalSubmission { get; set; } = "";
        public string CloneTickets { get; set; } = string.Empty;
        public bool IsNonCodeRelease { get; set; }
        public List<string> TargetEnvs { get; set; } = new List<string>
    {
        "QA Now",
        "One-Stg Upon QA Approval",
        "EU-Prod Upon QA Approval",
        "US-Prod Upon QA Approval"
    };


    }
}
