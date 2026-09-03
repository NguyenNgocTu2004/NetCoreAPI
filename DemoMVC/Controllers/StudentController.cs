using Microsoft.AspNetCore.Mvc;

namespace DemoMVC.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string fullName, string school, string address)
        {
            string hoTen = fullName;

            ViewBag.FullName = fullName;
            ViewBag.School = school;
            ViewBag.Address = address;
            ViewBag.HoTen = hoTen;

            return View();
        }
    }
}