namespace WebApplication1.Models;

   
    public class ServerInfo
    {
    public List<ServerInfo> Servers { get; set; }
    public List<string> Environments { get; set; }
    public List<string> Applications { get; set; }
    public string EnvFilter { get; set; } = "ALL";
    public string AppFilter { get; set; } = "ALL";
    public string SortExpression { get; set; }
    public string SortDirection { get; set; } = "ASC";
    public string Server { get; set; }
    public string ServerIP { get; set; }
    public string Environment { get; set; }
    public string Application { get; set; }
}
