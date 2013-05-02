using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace TplLunchAndLearn.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public async Task<ActionResult> SlowEcho(object input)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            return Json(input);
        }
    }
}
