using Microsoft.AspNetCore.Mvc;
using static System.Net.WebRequestMethods;

namespace WebApplication1.Controllers
{
    public class DMCR : Controller
    {
        public IActionResult DMCRRequest()
        {
            string redirectUrl = "https://qltlbbrscm02.corp.brassring.com/bldwebhost/DmcrIdentity.aspx";
            ViewBag.RedirectUrl = redirectUrl;
            return View();
        }
        public IActionResult DMCRTracker()
        {
            string redirectUrl = "https://qltlbbrscm02.corp.brassring.com/bldwebhost/DmcrTracker.aspx";
            ViewBag.RedirectUrl = redirectUrl;
            return View();
        }
        public IActionResult DMCRApproval()
        {
            string redirectUrl = "https://qltlbbrscm02.corp.brassring.com/bldwebhost/DmcrApproval.aspx";
            ViewBag.RedirectUrl = redirectUrl;
            return View();
        }
        public IActionResult ObjectConflict()
        {
            string redirectUrl = "https://qltlbbrscm02.corp.brassring.com/bldwebhost/ObjectConflict.aspx";
            ViewBag.RedirectUrl = redirectUrl;
            return View();
        }

    }
}
