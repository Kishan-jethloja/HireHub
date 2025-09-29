using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlacementManagementSystem.Data;
using PlacementManagementSystem.Models;
using PlacementManagementSystem.ViewModels;
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
		public IActionResult Profile()
		{
			var userId = _userManager.GetUserId(User);
			var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
			if (user == null || user.UserType != UserType.Company)
			{
				return Forbid();
			}
			var company = _db.Companies.FirstOrDefault(c => c.UserId == user.Id) ?? new Company { UserId = user.Id };
			return View(company);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Profile(Company model)
		{
			var userId = _userManager.GetUserId(User);
			var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
			if (user == null || user.UserType != UserType.Company)
			{
				return Forbid();
			}

			// UserId comes from the logged-in user, not the form
			ModelState.Remove("UserId");
			ModelState.Remove("Id");

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
			_db.SaveChanges();
			TempData["Success"] = "Profile updated.";
			return RedirectToAction("Profile");
		}

		[HttpGet]
		public IActionResult Public(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				return NotFound();
			}

			var company = _db.Companies.FirstOrDefault(c => c.UserId == id);
			if (company == null)
			{
				return NotFound();
			}

			var jobs = _db.JobPostings
				.Where(j => j.CompanyUserId == id)
				.OrderByDescending(j => j.CreatedAtUtc)
				.ToList();

			ViewBag.Jobs = jobs;
			return View(company);
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
		public IActionResult CreateJob(JobPosting model)
		{
			var userId = _userManager.GetUserId(User);
			var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
			if (user == null || user.UserType != UserType.Company)
			{
				return Forbid();
			}

			// Validate application deadline is not in the past
			if (model.ApplyByUtc.HasValue && model.ApplyByUtc.Value.Date < DateTime.Today)
			{
				ModelState.AddModelError("ApplyByUtc", "Application deadline cannot be in the past.");
			}

			// Duration validation: Internship => months required; FullTime => years required
			if (model.Type == JobType.Internship)
			{
				if (!model.DurationMonths.HasValue || model.DurationMonths.Value <= 0)
				{
					ModelState.AddModelError("DurationMonths", "Please enter duration in months for an internship.");
				}
				// Ensure the other unit is cleared
				model.DurationYears = null;
			}
			else if (model.Type == JobType.FullTime)
			{
				if (!model.DurationYears.HasValue || model.DurationYears.Value <= 0)
				{
					ModelState.AddModelError("DurationYears", "Please enter duration in years for a full-time role.");
				}
				// Ensure the other unit is cleared
				model.DurationMonths = null;
			}

			if (!ModelState.IsValid)
			{
				// Re-populate colleges dropdown on validation error
				ViewBag.Colleges = _db.Colleges.Select(c => c.Name).OrderBy(n => n).ToList();
				return View(model);
			}

			model.CompanyUserId = user.Id;
			_db.JobPostings.Add(model);
			_db.SaveChanges();
			TempData["Success"] = "Job/Internship posted successfully.";
			return RedirectToAction("MyJobs");
		}

		[HttpGet]
		public IActionResult MyJobs()
		{
			var userId = _userManager.GetUserId(User);
			var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
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
		public IActionResult Applications(int id)
		{
			var userId = _userManager.GetUserId(User);
			var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
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
		public IActionResult Announcements(int id)
		{
			var userId = _userManager.GetUserId(User);
			var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
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
		public IActionResult CreateAnnouncement(int id)
		{
			var userId = _userManager.GetUserId(User);
			var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
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
		public IActionResult CreateAnnouncement(int id, Announcement model)
		{
			var userId = _userManager.GetUserId(User);
			var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
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
				_db.SaveChanges();
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
		public IActionResult ApplicationDetails(int id)
		{
			var userId = _userManager.GetUserId(User);
			var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
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
		public IActionResult Hire(int id)
		{
			var userId = _userManager.GetUserId(User);
			var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
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
			_db.SaveChanges();
			TempData["Success"] = "Applicant marked as Hired.";
			return RedirectToAction("ApplicationDetails", new { id });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Reject(int id)
		{
			var userId = _userManager.GetUserId(User);
			var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
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
			_db.SaveChanges();
			TempData["Success"] = "Applicant rejected.";
			return RedirectToAction("ApplicationDetails", new { id });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult DeleteJob(int id)
		{
			var userId = _userManager.GetUserId(User);
			var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
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
			_db.SaveChanges();

			TempData["Success"] = "Job posting deleted successfully.";
			return RedirectToAction("MyJobs");
		}

		public IActionResult Feedback(int? jobId = null)
		{
			var userId = _userManager.GetUserId(User);
			var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
			if (user?.UserType != UserType.Company)
			{
				return Forbid();
			}

			// Get company's feedback
			var company = _db.Companies.FirstOrDefault(c => c.UserId == user.Id);
			if (company == null)
			{
				return NotFound();
			}

			// Get job title if filtering by specific job
			string jobTitle = null;
			if (jobId.HasValue)
			{
				var job = _db.JobPostings.FirstOrDefault(j => j.Id == jobId.Value && j.CompanyUserId == user.Id);
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

			var feedback = feedbackQuery
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
				.ToList();

			ViewBag.JobTitle = jobTitle;
			ViewBag.IsFilteredByJob = jobId.HasValue;
			return View(feedback);
		}

		[HttpGet]
		public IActionResult JobDetails(int id)
		{
			var userId = _userManager.GetUserId(User);
			var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
			if (user == null || user.UserType != UserType.Company)
			{
				return Forbid();
			}

			var job = _db.JobPostings.FirstOrDefault(j => j.Id == id && j.CompanyUserId == user.Id);
			if (job == null)
			{
				return NotFound();
			}

			var company = _db.Companies.FirstOrDefault(c => c.UserId == user.Id);
			ViewBag.CompanyName = company?.CompanyName ?? "My Company";
			return View(job);
		}

	}
}
