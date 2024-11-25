namespace WebApplication1.Models
{
    public class FilterViewModel
    {
        public string BuildPattern { get; set; }
        public bool ShowInactive { get; set; }
        public bool ShowUnreleased { get; set; }
        public bool Tickets { get; set; }
        public bool ProdOnly { get; set; }
        public List<string> EnvironmentAbbrevs { get; set; }
        public List<BuildModel> Builds { get; set; }
    }
}
