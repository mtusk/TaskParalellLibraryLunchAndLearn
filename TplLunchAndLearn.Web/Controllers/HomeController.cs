using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace TplLunchAndLearn.Web.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> SlowEcho(TimeSpan delay, string input)
        {
            if (delay != null)
            {
                await Task.Delay(delay);
            }
            
            return Content(input);
        }
    }
}
