using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace TplLunchAndLearn.Web.Controllers
{
    public class TplController : Controller
    {
        // GET: /Tpl/Details/5
        public async Task<int> SlowEcho(int id)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            return id;
        }
    }
}
