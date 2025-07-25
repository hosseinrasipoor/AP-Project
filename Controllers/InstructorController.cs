using Microsoft.AspNetCore.Mvc;

namespace Golestan.Controllers
{
    public class InstructorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
