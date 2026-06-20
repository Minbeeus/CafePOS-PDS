using Microsoft.AspNetCore.Mvc;

namespace CafePOS.Web.Controllers
{
    [Route("Admin")]
    public class AdminController : Controller
    {
        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("Login")]
        public IActionResult Login()
        {
            return View();
        }
    }
}
