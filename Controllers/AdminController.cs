using Golestan.Models;
using Microsoft.AspNetCore.Mvc;
using UniversityManager.Data;

namespace Golestan.Controllers
{
    public class AdminController : Controller
    {
        private readonly GolestanContext _context;

        public AdminController(GolestanContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
