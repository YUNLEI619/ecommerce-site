using Microsoft.AspNetCore.Mvc;
using ShoppingCart_T8.Models;
using System.Diagnostics;

namespace ShoppingCart_T8.Controllers
{
    public class StatsController : Controller //Taken largely from ASP .NET Workshop, modified slightly
    {
        private readonly LogStats _stats;

        public StatsController(LogStats stats)
        {
            _stats = stats;
        }

        public IActionResult Index() => RedirectToAction("Login", "Customer");

        public IActionResult NotFound() => View("NotFound"); //custom error page for user

        public IActionResult EasterEgg() => View(_stats); //View("NotFound");

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}