//using Microsoft.AspNetCore.Mvc; // For ASP.NET Core MVC
//// using System.Web.Mvc; // For ASP.NET MVC (Non-Core)
//using WebApplication1.Services;

//namespace WebApplication1.Controllers
//{
//    public class ScriptController : Controller
//    {
//        private readonly ScriptService _scriptService;

//        public ScriptController(ScriptService scriptService)
//        {
//            _scriptService = scriptService;
//        }

//        public IActionResult ScriptDetails() // Changed ActionResult to IActionResult for ASP.NET Core
//        {
//            return View();
//        }

//        [HttpPost]
//        public IActionResult GetScriptDetail(string scriptName) // Changed ActionResult to IActionResult for ASP.NET Core
//        {
//            var scriptDetail = _scriptService.GetScriptDetail(scriptName);
//            return PartialView("_ScriptDetailPartial", scriptDetail);
//        }
//    }
//}


//using Azure.Core;
//using Microsoft.AspNetCore.Mvc;
//using System.Data;


//public class ScriptController : Controller
//{
//    private readonly ScriptRepository _repository;

//    public ScriptController(ScriptRepository repository)
//    {
//        _repository = repository;
//    }

//    public ActionResult Details(string scriptName)
//    {
//        DataSet data = _repository.GetScriptsData();

//        // Check if the request is an AJAX request
//        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
//        {
//            return PartialView("_ScriptDetailPartial", data);
//        }

//        return View(data);
//    }
//}


//using Microsoft.AspNetCore.Mvc;
//using System.Data;

//public class ScriptController : Controller
//{
//    private readonly ScriptRepository _repository;

//    public ScriptController(ScriptRepository repository)
//    {
//        _repository = repository;
//    }

//    public ActionResult Details(string scriptName)
//    {
//        DataSet data = _repository.GetScriptData(scriptName);

//        // Check if the request is an AJAX request
//        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
//        {
//            return PartialView("_ScriptDetailPartial", data);
//        }

//        return View(data);
//    }
//}


//working//


using Microsoft.AspNetCore.Mvc;
using System.Data;

public class ScriptController : Controller
{
    private readonly ScriptRepository _repository;

    public ScriptController(ScriptRepository repository)
    {
        _repository = repository;
    }

    public ActionResult Details(string scriptName = "defaultScriptName")
    {
        if (string.IsNullOrEmpty(scriptName))
        {
            return BadRequest("Script name cannot be null or empty.");
        }

        DataSet data = _repository.GetScriptData(scriptName);

        // Check if the request is an AJAX request
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_ScriptDetailPartial", data);
        }

        return View(data);
    }
}

