using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class NonCodeRelease : Controller
    {
            public IActionResult TriggerNonCodeRelease()
            {
                // Set the necessary view bags to simulate a non-code release request
                ViewBag.NonCodeRelease = "YES";
                ViewBag.Title = "None-Code Release Request";

                // Force the non-code release behavior by passing an empty IID
                HttpContext.Session.SetString("IID", string.Empty);

                // Instantiate a new ReleaseRequestViewModel to pass to the view
                var releaseRequestViewModel = new ReleaseRequestViewModel
                {
                    Branch = string.Empty, // No branch for non-code release
                    TargetEnv = string.Empty, // No target environment
                    BuildLabel = string.Empty // No build label for non-code release
                };

                // Return the view with the non-code release model
                return View("Index", releaseRequestViewModel);
            }

           
        }
    }

