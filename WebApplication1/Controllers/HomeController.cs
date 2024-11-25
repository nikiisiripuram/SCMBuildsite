using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Diagnostics;
using WebApplication1.Models;
using System.Security.Claims;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BuildRepository _buildRepository;

        public HomeController(ILogger<HomeController> logger, BuildRepository buildRepository)
        {
            _logger = logger;
            _buildRepository = buildRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Github()
        {
            string email = UserPrincipal.Current.EmailAddress;
            ViewBag.Email = email;
            return View();
        }

        public IActionResult DeployDevBuild()
        {
            string email = UserPrincipal.Current.EmailAddress;
            ViewBag.Email = email;

            if (!string.IsNullOrEmpty(email))
            {
                List<string> labels = _buildRepository.GetLabelsByEmail(email);
                ViewBag.Labels = labels;
            }
            else
            {
                ViewBag.Labels = new List<string>();
            }

            return View();
        }

        [HttpPost]
        public JsonResult FetchBuildLabels(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                List<string> labels = _buildRepository.GetLabelsByEmail(email);
                return Json(new { labels = labels });
            }
            return Json(new { labels = new List<string>() });
        }

        public IActionResult DeployDevConfig()
        {
            string email = UserPrincipal.Current.EmailAddress;
            ViewBag.Email = email;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public IActionResult NonCodeRelease()
        {
            string redirectUrl = "https://qltlbbrscm02.corp.brassring.com/coderelease/ReleaseRequest.aspx";
            string email = UserPrincipal.Current.EmailAddress;  // Retrieve the user's email
            ViewBag.RedirectUrl = redirectUrl;
            ViewBag.Email = email;  // Pass email to the view
            return View();
        }
        public IActionResult Unauthorized()
        {
            return View(); // This will look for Views/Shared/Unauthorized.cshtml
        }
        public IActionResult OMGDashBoard()
        {
            string redirectUrl = "https://qltlbbrscm02.corp.brassring.com/OMG-Monitors/DashboardForm.aspx";
            string email = UserPrincipal.Current.EmailAddress;  // Retrieve the user's email
            ViewBag.RedirectUrl = redirectUrl;
            ViewBag.Email = email;  // Pass email to the view
            return View();
            
        }
    }
}


