using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class LuffyController : Controller
    {
        private readonly ServerService _serverService;

        public LuffyController(ServerService serverService)
        {
            _serverService = serverService;
        }

        public IActionResult Server(string environment = "All", string application = "All")
        {
            var servers = _serverService.FetchServers(environment, application);
            return View(servers);
        }
    }
}
