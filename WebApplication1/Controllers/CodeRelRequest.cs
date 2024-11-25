using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class CodeRelRequestController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly BuildRepository _repository;

        public CodeRelRequestController(IConfiguration configuration, BuildRepository repository)
        {
            _configuration = configuration;
            _repository = repository;
        }

        public IActionResult Build(string branch)
        {
            List<Build> builds;
            if (!string.IsNullOrEmpty(branch))
            {
                builds = _repository.GetEligibleBuilds(branch);
            }
            else
            {
                builds = _repository.GetEligibleBuilds();
            }

            return View(builds);
        }
        public IActionResult Build2(string branch)
        {
            List<Build> builds;
            if (!string.IsNullOrEmpty(branch))
            {
                builds = _repository.GetEligibleBuilds(branch);
            }
            else
            {
                builds = _repository.GetEligibleBuilds();
            }

            return View(builds);
        }

        [HttpGet]
        public JsonResult FilterBuilds(string branch)
        {
            var builds = _repository.GetEligibleBuilds(branch);
            return Json(builds);
        }

        public ActionResult CodeRequest(string IID, string Specific)
        {
            // Construct the URL with query parameters IID and Specific
            string redirectUrl = $"https://qltlbbrscm02.corp.brassring.com/coderelease/ReleaseRequest.aspx?IID={IID}&Specific={Specific}";


            // Pass the URL to the view via ViewBag
            ViewBag.RedirectUrl = redirectUrl;

            return View();
        }


    }
}
    







