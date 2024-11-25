//using Microsoft.AspNetCore.Mvc;
//using System.DirectoryServices.AccountManagement;
//using System.Security.Policy;

//namespace WebApplication1.Controllers
//{
//    public class SmokeTest : Controller
//    {

//        public IActionResult EUprod()
//        {
//            string redirectUrl = "https://seu01brscm01.ops-eu.hosting-eu.knxa/Azure2xBSmokeTest/";

//            // Create a JavaScript snippet to open the URL in a new tab and then redirect to the home page
//            string script = @"
//                    <script type='text/javascript'>
//                        // Open the URL in a new tab
//                        var newTab = window.open('" + redirectUrl + @"', '_blank');

//                        // Focus the new tab (if popups are allowed)
//                        if (newTab) {
//                            newTab.focus();
//                        } else {
//                            alert('Please allow popups for this website');
//                        }

//                        // Redirect the current page to the home page
//                        window.location.href = '" + Url.Action("Index", "Home") + @"';
//                    </script>";

//            // Return the JavaScript content as a ContentResult
//            return Content(script, "text/html");
//        }
//        public IActionResult USprod()
//        {
//            string redirectUrl = "https://pst01brscm01.prod.brassring.com/Azure2xBSmokeTest";

//            // Create a JavaScript snippet to open the URL in a new tab and then redirect to the home page
//            string script = @"
//                    <script type='text/javascript'>
//                        // Open the URL in a new tab
//                        var newTab = window.open('" + redirectUrl + @"', '_blank');

//                        // Focus the new tab (if popups are allowed)
//                        if (newTab) {
//                            newTab.focus();
//                        } else {
//                            alert('Please allow popups for this website');
//                        }

//                        // Redirect the current page to the home page
//                        window.location.href = '" + Url.Action("Index", "Home") + @"';
//                    </script>";

//            // Return the JavaScript content as a ContentResult
//            return Content(script, "text/html");
//        }

//    }

//    public IActionResult OneStg()
//    {
//        string redirectUrl = Url.Encode("https://spk01brscm02.prod.brassring.com/Azure2xBServerMonitor/WBMonitor.aspx?env=stg");

//        string script = $@"
//        <script type='text/javascript'>
//            // Open the URL in a new tab
//            var newTab = window.open('{redirectUrl}', '_blank');

//            // Focus the new tab (if popups are allowed)
//            if (newTab) {{
//                newTab.focus();
//            }} else {{
//                alert('Please allow popups for this website.');
//            }}

//            // Redirect the current page to the home page
//            window.location.href = '{Url.Action("Index", "Home")}';
//        </script>";

//        // Return the JavaScript content as a ContentResult
//        return Content(script, "text/html");
//    }


//}


using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class SmokeTest : Controller
    {
        public IActionResult EUprod()
        {
            string redirectUrl = "https://seu01brscm01.ops-eu.hosting-eu.knxa/Azure2xBSmokeTest/";
            string script = $@"
                <script type='text/javascript'>
                    var newTab = window.open('{redirectUrl}', '_blank');
                    if (newTab) {{
                        newTab.focus();
                    }} else {{
                        alert('Please allow popups for this website.');
                    }}
                    window.location.href = '{Url.Action("DevOpes2", "Auth")}';
                </script>";
            return Content(script, "text/html");
        }

        public IActionResult USprod()
        {
            string redirectUrl = "https://pst01brscm01.prod.brassring.com/Azure2xBSmokeTest";
            string script = $@"
                <script type='text/javascript'>
                    var newTab = window.open('{redirectUrl}', '_blank');
                    if (newTab) {{
                        newTab.focus();
                    }} else {{
                        alert('Please allow popups for this website.');
                    }}
                    window.location.href = '{Url.Action("DevOpes2", "Auth")}';
                </script>";
            return Content(script, "text/html");
        }

        public IActionResult OneStg()
        {
            string redirectUrl = "https://spk01brscm02.prod.brassring.com/Azure2xBServerMonitor/WBMonitor.aspx?env=stg";
            string script = $@"
                <script type='text/javascript'>
                    var newTab = window.open('{redirectUrl}', '_blank');
                    if (newTab) {{
                        newTab.focus();
                    }} else {{
                        alert('Please allow popups for this website.');
                    }}
                    window.location.href = '{Url.Action("DevOpes2", "Auth")}';
                </script>";
            return Content(script, "text/html");
        }
        public IActionResult QA()
        {
            string redirectUrl = "https://qltlbbrscm02.corp.brassring.com/2xBSmokeTest/WBMonitor.aspx?env=QA";
            string script = $@"
                <script type='text/javascript'>
                    var newTab = window.open('{redirectUrl}', '_blank');
                    if (newTab) {{
                        newTab.focus();
                    }} else {{
                        alert('Please allow popups for this website.');
                    }}
                    window.location.href = '{Url.Action("DevOpes2", "Auth")}';
                </script>";
            return Content(script, "text/html");
        }

    }
}
