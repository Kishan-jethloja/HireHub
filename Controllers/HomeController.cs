using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using PlacementManagementSystem.Models;
using PlacementManagementSystem.Data;
using System.Threading.Tasks;
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

        public async Task<IActionResult> Index()
        {
            ViewBag.ShowProfileBanner = false;
            var user = await _userManager.GetUserAsync(User);
            if (user != null && user.UserType == UserType.Student)
            {
                // Land students on Jobs by default
                return RedirectToAction("Jobs", "Student");
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(string name, string email, string subject, string message)
        {
            TempData["Success"] = "Thank you for contacting us. We'll get back to you soon.";
            return RedirectToAction("Contact");
        }

        [HttpGet]
        public IActionResult Feedback()
        {
            ViewBag.Companies = _db.Companies.Select(c => new { c.Id, c.CompanyName }).ToList();
            return View(new Feedback());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Feedback(Feedback model)
        {
            ViewBag.Companies = _db.Companies.Select(c => new { c.Id, c.CompanyName }).ToList();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Please login to submit feedback.");
                return View(model);
            }

            // Enforce role-based target
            if (user.UserType == UserType.Student)
            {
                model.TargetType = FeedbackTargetType.Company;
                if (model.TargetCompanyId == null)
                {
                    ModelState.AddModelError(nameof(model.TargetCompanyId), "Please select a company.");
                    return View(model);
                }
                model.TargetCollegeName = null;
            }
            else if (user.UserType == UserType.Company)
            {
                model.TargetType = FeedbackTargetType.College;
                model.TargetCompanyId = null;
                if (string.IsNullOrWhiteSpace(model.TargetCollegeName))
                {
                    ModelState.AddModelError(nameof(model.TargetCollegeName), "Please enter the college name.");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Only students and companies can submit feedback.");
                return View(model);
            }

            model.AuthorUserId = user.Id;

            _db.Feedbacks.Add(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Feedback submitted successfully.";
            return RedirectToAction("Feedback");
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
