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
		public async Task<IActionResult> Profile()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null || user.UserType != UserType.Company)
			{
				return Forbid();
			}
			var company = _db.Companies.FirstOrDefault(c => c.UserId == user.Id) ?? new Company { UserId = user.Id };
			return View(company);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Profile(Company model)
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
			var company = _db.Companies.FirstOrDefault(c => c.UserId == user.Id);
			if (company == null)
			{
				company = new Company
				{
					UserId = user.Id,
					CompanyName = model.CompanyName,
					Description = model.Description,
					Website = model.Website,
					Industry = model.Industry,
					Address = model.Address
				};
				_db.Companies.Add(company);
			}
			else
			{
				company.CompanyName = model.CompanyName;
				company.Description = model.Description;
				company.Website = model.Website;
				company.Industry = model.Industry;
				company.Address = model.Address;
			}
			await _db.SaveChangesAsync();
			TempData["Success"] = "Profile updated.";
			return RedirectToAction("Profile");
		}

		[HttpGet]
		public IActionResult CreateJob()
		{
			// Provide registered colleges for the dropdown
			ViewBag.Colleges = _db.Colleges.Select(c => c.Name).OrderBy(n => n).ToList();
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
				// Re-populate colleges dropdown on validation error
				ViewBag.Colleges = _db.Colleges.Select(c => c.Name).OrderBy(n => n).ToList();
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

		[HttpGet]
		public async Task<IActionResult> Applications(int id)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null || user.UserType != UserType.Company)
			{
				return Forbid();
			}

			var job = _db.JobPostings.FirstOrDefault(j => j.Id == id && j.CompanyUserId == user.Id);
			if (job == null)
			{
				return NotFound();
			}

			var applications = _db.Applications
				.Where(a => a.JobPostingId == id)
				.OrderByDescending(a => a.CreatedAtUtc)
				.ToList();

			ViewBag.Job = job;
			return View(applications);
		}

		[HttpGet]
		public async Task<IActionResult> ApplicationDetails(int id)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null || user.UserType != UserType.Company)
			{
				return Forbid();
			}

			var app = _db.Applications.FirstOrDefault(a => a.Id == id);
			if (app == null)
			{
				return NotFound();
			}
			var job = _db.JobPostings.FirstOrDefault(j => j.Id == app.JobPostingId);
			if (job == null || job.CompanyUserId != user.Id)
			{
				return Forbid();
			}

			ViewBag.Job = job;
			return View(app);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteJob(int id)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null || user.UserType != UserType.Company)
			{
				return Forbid();
			}

			var job = _db.JobPostings.FirstOrDefault(j => j.Id == id && j.CompanyUserId == user.Id);
			if (job == null)
			{
				return NotFound();
			}

			// Delete related applications first
			var applications = _db.Applications.Where(a => a.JobPostingId == id).ToList();
			_db.Applications.RemoveRange(applications);

			// Delete the job posting
			_db.JobPostings.Remove(job);
			await _db.SaveChangesAsync();

			TempData["Success"] = "Job posting deleted successfully.";
			return RedirectToAction("MyJobs");
		}
	}
}
