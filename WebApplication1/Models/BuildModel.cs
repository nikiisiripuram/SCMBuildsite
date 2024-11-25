//namespace WebApplication1.Models
//{
//    public class BuildModel
//    {
//        public int Id { get; set; }
//        public string BuildOldStyle { get; set; }
//        public string BuildNewStyle { get; set; }
//        public string Label { get; set; }
//        public DateTime DateAdded { get; set; }
//        public string BG_USER_18 { get; set; }
//        public bool SpecialInstructions { get; set; }
//        public string ConfigChanges { get; set; }
//        public string[] EnvAbbreviations { get; set; } = new string[4]; // CQA, CSTG, PR, EU
//        public bool HasSpecialInstructionsOrConfigChanges { get; set; } // For determining if the SI link should be shown
//        public string SILink { get; set; } // The actual SI link to be rendered

//        // Array to hold environment abbreviations

//        public List<EnvironmentModel> Environments { get; set; } = new List<EnvironmentModel>();
//    }
//}


public class BuildModel
{
    public int Id { get; set; }
    public string BuildOldStyle { get; set; }
    public string BuildNewStyle { get; set; }
    public string Label { get; set; }
    public DateTime DateAdded { get; set; }
    // Changed from array to List<string>
    public bool HasSpecialInstructionsOrConfigChanges { get; set; }
    public string[] EnvAbbreviations { get; set; } = new string[4]; // CQA, CSTG, PR, EU
    public string SILink { get; set; }
    public string SpecialInstructions { get; set; }
    public string ConfigChanges { get; set; }
}
