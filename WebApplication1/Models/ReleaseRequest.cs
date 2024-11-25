namespace WebApplication1.Models
{
    public class ReleaseRequestModel
    {
        public string FinalSubmission { get; set; }
        public string NonCodeRelease { get; set; }
        public string Title { get; set; }
        public string NCReleaseProduct { get; set; }
        public string BuildLabel { get; set; }
        public string Branch { get; set; }
        public string WorkItemsDBScripts { get; set; }
        public string WorkItemsSI { get; set; }
        public string IID { get; set; }
        public string WorkItems { get; set; }
        public string ContactEmail { get; set; }
        public string TargetEnv { get; set; }
    }

    public class BuildDetails
    {
        public string BuildLabel { get; set; }
        public string Branch { get; set; }
    }
}
