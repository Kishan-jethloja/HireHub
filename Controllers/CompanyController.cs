using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlacementManagementSystem.Data;
using PlacementManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;

namespace PlacementManagementSystem.Controllers
{
	[Authorize]
	public class CompanyController : Controller
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;

		public CompanyController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
		{
			_db = db;
			_userManager = userManager;
		}

		[HttpGet]
		public IActionResult CreateJob()
		{
			return View(new JobPosting());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateJob(JobPosting model)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null || user.UserType != UserType.Company)
			{
				return Forbid();
			}

			if (!ModelState.IsValid)
			{
				return View(model);
			}

			model.CompanyUserId = user.Id;
			_db.JobPostings.Add(model);
			await _db.SaveChangesAsync();
			TempData["Success"] = "Job/Internship posted successfully.";
			return RedirectToAction("MyJobs");
		}

		[HttpGet]
		public async Task<IActionResult> MyJobs()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null || user.UserType != UserType.Company)
			{
				return Forbid();
			}
			var jobs = _db.JobPostings.Where(j => j.CompanyUserId == user.Id)
				.OrderByDescending(j => j.CreatedAtUtc)
				.ToList();
			return View(jobs);
		}
	}
}
