//using Microsoft.AspNetCore.Mvc;
//using System.DirectoryServices.AccountManagement;

//namespace WebApplication1.Controllers
//{
//    public class BigIpStatus : Controller
//    {
//        public IActionResult USBigIpStatus()
//        {
//            string redirectUrl = "https://pst01brscm01.prod.brassring.com/AzureUSBigIp/Default.aspx";
//            string script = $@"
//                <script type='text/javascript'>
//                    var newTab = window.open('{redirectUrl}', '_blank');
//                    if (newTab) {{
//                        newTab.focus();
//                    }} else {{
//                        alert('Please allow popups for this website.');
//                    }}
//                    window.location.href = '{Url.Action("DevOpes2", "Auth")}';
//                </script>";
//            return Content(script, "text/html");
//        }

//        public IActionResult EUBigIpStatus()
//        {
//            string redirectUrl = "https://seu01brscm01.ops-eu.hosting-eu.knxa/AzureEUBigIP/Default.aspx";
//            string script = $@"
//                <script type='text/javascript'>
//                    var newTab = window.open('{redirectUrl}', '_blank');
//                    if (newTab) {{
//                        newTab.focus();
//                    }} else {{
//                        alert('Please allow popups for this website.');
//                    }}
//                    window.location.href = '{Url.Action("DevOpes2", "Auth")}';
//                </script>";
//            return Content(script, "text/html");
//        }
//        public IActionResult StgBigIpStatus()
//        {
//            string redirectUrl = "https://spk01brscm02.prod.brassring.com/AzureStagingBigIp/Default.aspx";
//            string script = $@"
//                <script type='text/javascript'>
//                    var newTab = window.open('{redirectUrl}', '_blank');
//                    if (newTab) {{
//                        newTab.focus();
//                    }} else {{
//                        alert('Please allow popups for this website.');
//                    }}
//                    window.location.href = '{Url.Action("DevOpes2", "Auth")}';
//                </script>";
//            return Content(script, "text/html");
//        }
//        public IActionResult QABigIpStatus()
//        {
//            string redirectUrl = "https://spk01brscm02.prod.brassring.com/AzureStagingBigIp/Default.aspx";
//            string script = $@"
//                <script type='text/javascript'>
//                    var newTab = window.open('{redirectUrl}', '_blank');
//                    if (newTab) {{
//                        newTab.focus();
//                    }} else {{
//                        alert('Please allow popups for this website.');
//                    }}
//                    window.location.href = '{Url.Action("DevOpes2", "Auth")}';
//                </script>";
//            return Content(script, "text/html");
//        }
//    }
//}



using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace WebApplication1.Controllers
{
    public class BigIpStatus : Controller
    {
        private readonly IMemoryCache _cache;

        // Constructor to inject MemoryCache
        public BigIpStatus(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        public IActionResult USBigIpStatus()
        {
            string redirectUrl = GetCachedUrl("USBigIpStatus", "https://pst01brscm01.prod.brassring.com/AzureUSBigIp/Default.aspx");
            return RedirectWithPopup(redirectUrl);
        }

        public IActionResult EUBigIpStatus()
        {
            string redirectUrl = GetCachedUrl("EUBigIpStatus", "https://seu01brscm01.ops-eu.hosting-eu.knxa/AzureEUBigIP/Default.aspx");
            return RedirectWithPopup(redirectUrl);
        }

        public IActionResult StgBigIpStatus()
        {
            string redirectUrl = GetCachedUrl("StgBigIpStatus", "https://spk01brscm02.prod.brassring.com/AzureStagingBigIp/Default.aspx");
            return RedirectWithPopup(redirectUrl);
        }

        public IActionResult QABigIpStatus()
        {
            string redirectUrl = GetCachedUrl("QABigIpStatus", "https://spk01brscm02.prod.brassring.com/AzureStagingBigIp/Default.aspx");
            return RedirectWithPopup(redirectUrl);
        }

        // Helper method to cache URLs in memory for faster access
        private string GetCachedUrl(string cacheKey, string defaultUrl)
        {
            // Check if the URL is already cached
            if (!_cache.TryGetValue(cacheKey, out string cachedUrl))
            {
                // If not cached, cache the URL with an absolute expiration of 1 hour
                _cache.Set(cacheKey, defaultUrl, TimeSpan.FromHours(1));
                cachedUrl = defaultUrl;
            }
            return cachedUrl;
        }

        // Helper method to return a popup redirection with optional fallback
        private IActionResult RedirectWithPopup(string redirectUrl)
        {
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

