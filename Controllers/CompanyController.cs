using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlacementManagementSystem.Data;
using PlacementManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using System;

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
		public async Task<IActionResult> Announcements(int id)
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

			var anns = _db.Announcements
				.Where(a => a.JobPostingId == id)
				.OrderByDescending(a => a.CreatedAtUtc)
				.ToList();
			ViewBag.Job = job;
			return View(anns);
		}

		[HttpGet]
		public async Task<IActionResult> CreateAnnouncement(int id)
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
			ViewBag.Job = job;
			var eligibleCount = _db.Applications
				.Where(a => a.JobPostingId == job.Id && a.Status != ApplicationStatus.Rejected)
				.Select(a => a.StudentUserId)
				.Distinct()
				.Count();
			ViewBag.EligibleRecipientCount = eligibleCount;
			return View(new Announcement());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateAnnouncement(int id, Announcement model)
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

			// These are set by server, not posted from the form
			ModelState.Remove("JobPostingId");
			ModelState.Remove("CompanyUserId");
			if (!ModelState.IsValid)
			{
				ViewBag.Job = job;
				ViewBag.EligibleRecipientCount = _db.Applications
					.Where(a => a.JobPostingId == job.Id && a.Status != ApplicationStatus.Rejected)
					.Select(a => a.StudentUserId)
					.Distinct()
					.Count();
				return View(model);
			}

			var recipientCount = _db.Applications
				.Where(a => a.JobPostingId == job.Id && a.Status != ApplicationStatus.Rejected)
				.Select(a => a.StudentUserId)
				.Distinct()
				.Count();
			if (recipientCount == 0)
			{
				ModelState.AddModelError(string.Empty, "There are no eligible applicants to receive this announcement yet.");
				ViewBag.Job = job;
				ViewBag.EligibleRecipientCount = 0;
				return View(model);
			}

			var ann = new Announcement
			{
				JobPostingId = job.Id,
				CompanyUserId = user.Id,
				Title = model.Title,
				Message = model.Message,
				CreatedAtUtc = DateTime.UtcNow
			};
			_db.Announcements.Add(ann);
			try
			{
				await _db.SaveChangesAsync();
				TempData["Success"] = $"Announcement posted to {recipientCount} applicant(s).";
				return RedirectToAction("Announcements", new { id = job.Id });
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.InnerException?.Message ?? ex.Message);
				ViewBag.Job = job;
				ViewBag.EligibleRecipientCount = recipientCount;
				return View(model);
			}
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
		public async Task<IActionResult> Hire(int id)
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

			app.Status = ApplicationStatus.Hired;
			await _db.SaveChangesAsync();
			TempData["Success"] = "Applicant marked as Hired.";
			return RedirectToAction("ApplicationDetails", new { id });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Reject(int id)
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

			app.Status = ApplicationStatus.Rejected;
			await _db.SaveChangesAsync();
			TempData["Success"] = "Applicant rejected.";
			return RedirectToAction("ApplicationDetails", new { id });
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

		public async Task<IActionResult> Feedback(int? jobId = null)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user?.UserType != UserType.Company)
			{
				return Forbid();
			}

			// Get company's feedback
			var company = await _db.Companies.FirstOrDefaultAsync(c => c.UserId == user.Id);
			if (company == null)
			{
				return NotFound();
			}

			// Get job title if filtering by specific job
			string jobTitle = null;
			if (jobId.HasValue)
			{
				var job = await _db.JobPostings.FirstOrDefaultAsync(j => j.Id == jobId.Value && j.CompanyUserId == user.Id);
				if (job == null)
				{
					return NotFound();
				}
				jobTitle = job.Title;
			}

			var feedbackQuery = _db.Feedbacks
				.Where(f => f.TargetCompanyId == company.Id);

			// Filter by specific job if jobId is provided
			if (jobId.HasValue)
			{
				// Only show feedback for this specific job
				feedbackQuery = feedbackQuery.Where(f => f.JobPostingId == jobId.Value);
			}

			var feedback = await feedbackQuery
				.Include(f => f.AuthorUser)
				.Include(f => f.JobPosting)
				.OrderByDescending(f => f.CreatedAtUtc)
				.Select(f => new FeedbackViewModel
				{
					Id = f.Id,
					Subject = f.Subject,
					Message = f.Message,
					Rating = f.Rating,
					CreatedAt = f.CreatedAtUtc,
					AuthorName = $"{f.AuthorUser.FirstName} {f.AuthorUser.LastName}".Trim() != "" 
						? $"{f.AuthorUser.FirstName} {f.AuthorUser.LastName}".Trim()
						: f.AuthorUser.Email,
					JobTitle = f.JobPosting != null ? f.JobPosting.Title : (jobTitle ?? "General Feedback"),
					ApplicationStatus = "N/A" // Temporarily disabled
				})
				.ToListAsync();

			ViewBag.JobTitle = jobTitle;
			ViewBag.IsFilteredByJob = jobId.HasValue;
			return View(feedback);
		}

	}
}
