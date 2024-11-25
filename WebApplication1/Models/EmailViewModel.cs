namespace WebApplication1.Models
{
    public class EmailViewModel
    {
        public int IID { get; set; }
        public string EmailList { get; set; }
        public string Build { get; set; }
        public bool HadScripts { get; set; }
        public bool HasScripts { get; set; }
        public bool NonCode { get; set; }
        public string URLFROM { get; set; }
        public string EmailBody { get; set; } // Optional: To display the email content
        public string Subject { get; set; }
    }
}
