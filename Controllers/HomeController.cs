using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using PlacementManagementSystem.Models;
using PlacementManagementSystem.Data;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace PlacementManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
            
            if (user != null)
            {
                switch (user.UserType)
                {
                    case UserType.Student:
                        return RedirectToAction("Jobs", "Student");
                    case UserType.Company:
                        return RedirectToAction("MyJobs", "Company");
                    case UserType.College:
                        return RedirectToAction("Students", "College");
                }
            }
            
            // Show home page for unauthenticated users
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ErrorViewModel
    {
        public string RequestId { get; set; }
    }
}
