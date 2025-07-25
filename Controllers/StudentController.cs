using Microsoft.AspNetCore.Mvc;

namespace Golestan.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
