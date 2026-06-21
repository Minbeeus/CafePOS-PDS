using Microsoft.AspNetCore.Mvc;

namespace CafePOS.Web.Controllers
{
    [Route("PDS")]
    public class PDSController : Controller
    {
        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
