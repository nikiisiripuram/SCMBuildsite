//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Threading.Tasks;
//using WebApplication1.Models;

//namespace WebApplication2.Controllers
//{
//    public class AuthController : Controller
//    {
//        [Authorize(Policy = "WindowsPolicy")]
//        public async Task<IActionResult> DevOpes()
//        {
//            // Add your custom logic here for the specific user
//            return View();
//        }
//    }
//}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    public class AuthController : Controller
    {
        [Authorize(Policy = "WindowsPolicy")]
        public async Task<IActionResult> DevOpes()
        {
            // Get the URL you want to redirect to
            string redirectUrl = "https://qltlbbrscm02/scm/?src=builds.asp";

            // Create a JavaScript snippet to open the URL in a new tab and then redirect to the home page
            string script = @"
                <script type='text/javascript'>
                    // Open the URL in a new tab
                    var newTab = window.open('" + redirectUrl + @"', '_blank');

                    // Focus the new tab (if popups are allowed)
                    if (newTab) {
                        newTab.focus();
                    } else {
                        alert('Please allow popups for this website');
                    }

                    // Redirect the current page to the home page
                    window.location.href = '" + Url.Action("Index", "Home") + @"';
                </script>";

            // Return the JavaScript content as a ContentResult
            return Content(script, "text/html");
        }

       
        public  async Task<IActionResult> Devopes1()
        {
            return View();
        }

        [Authorize(Policy = "WindowsPolicy")]
        public async Task<IActionResult> Devopes2()
        {
            return View();
        }
        //public IActionResult OMGDashBoard()
        //{
        //    return View()
        //}
    }
}





//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Threading.Tasks;
//using WebApplication1.Models; 
// // Assuming you have models for user and authorization data

//namespace Auth.Controllers
//{
//    public class Auth : Controller
//    {
//        private readonly ScmDBContext _context; // Inject your DbContext here

//        public Auth(ScmDBContext context)
//        {
//            _context = context;
//        }

//        public class ScmDBContext : DbContext
//        {
//            public DbSet<UserCredential> UserCredentials { get; set; }
//            // Other DbSet properties

//            // DbContext constructor and configuration
//        }

//        [Authorize(Policy = "WindowsPolicy")]
//        public async Task<IActionResult> DevOpes()
//        {
//            // Get current user's Windows identity
//            var windowsIdentity = User.Identity as System.Security.Principal.WindowsIdentity;
//            if (windowsIdentity != null)
//            {
//                // Query database to check user authorization
//                var user = await _context.UserCredentials.FirstOrDefaultAsync(u => u.Username == windowsIdentity.Name);
//                if (user != null)
//                {
//                    // Check if user is authorized (assuming IsAuthorized is a property in UserCredential)
//                    if (user.IsAuthorized)
//                    {
//                        // User is authorized
//                        // Set session data
//                        //HttpContext.Session.SetString("UserId", user.UserId.ToString());
//                        HttpContext.Session.SetString("Username", user.Username);
//                        HttpContext.Session.SetString("Email", user.Email);
//                        // Add more session data as needed

//                        return View();
//                    }
//                }
//            }

//            // Handle unauthorized or error cases
//            return RedirectToAction("Unauthorized", "Error");
//        }
//    }
//}
