using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlacementManagementSystem.Data;
using PlacementManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace PlacementManagementSystem.Controllers
{
	[Authorize]
	public class CollegeController : Controller
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;

		public CollegeController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
		{
			_db = db;
			_userManager = userManager;
		}

		public class CompanySummary
		{
			public string CompanyUserId { get; set; }
			public string CompanyName { get; set; }
			public string Email { get; set; }
			public int JobsCount { get; set; }
			public System.DateTime LastPostedUtc { get; set; }
		}

		[HttpGet]
		public IActionResult Students(string college)
		{
			var user = _userManager.GetUserAsync(User).Result;
			if (user == null)
			{
				return Forbid();
			}

			// Default filter to the logged-in college (if this account owns a college)
			var ownedCollege = _db.Colleges.FirstOrDefault(c => c.CollegeUserId == user.Id)?.Name;
			var effectiveCollege = string.IsNullOrWhiteSpace(college) ? ownedCollege : college;
			var query = _db.Students
				.Include(s => s.User)
				.AsQueryable();
			if (!string.IsNullOrWhiteSpace(effectiveCollege))
			{
				query = query.Where(s => s.CollegeName == effectiveCollege);
			}

			var distinctColleges = _db.Students
				.Select(s => s.CollegeName)
				.Distinct()
				.OrderBy(n => n)
				.ToList();

			ViewBag.SelectedCollege = effectiveCollege ?? string.Empty;
			ViewBag.Colleges = distinctColleges;
			// Hide the filter for college accounts; admins can still use it
			ViewBag.ShowCollegeFilter = user.UserType != UserType.College;
			var students = query
				.OrderBy(s => s.Department)
				.ThenBy(s => s.StudentId)
				.ToList();
			return View(students);
		}

		[HttpGet]
		public IActionResult Companies()
		{
			var user = _userManager.GetUserAsync(User).Result;
			if (user == null || user.UserType != UserType.College)
			{
				return Forbid();
			}

			var ownedCollege = _db.Colleges.FirstOrDefault(c => c.CollegeUserId == user.Id)?.Name;
			if (string.IsNullOrWhiteSpace(ownedCollege))
			{
				TempData["Error"] = "Set up your college profile first.";
				return RedirectToAction("Profile");
			}

			var jobGroups = _db.JobPostings
				.Where(j => j.CollegeName == ownedCollege || j.CollegeName == "All Colleges")
				.GroupBy(j => j.CompanyUserId)
				.Select(g => new { CompanyUserId = g.Key, JobsCount = g.Count(), LastPostedUtc = g.Max(x => x.CreatedAtUtc) })
				.ToList();

			var companyUserIds = jobGroups.Select(g => g.CompanyUserId).ToList();
			var companyNames = _db.Companies
				.Where(c => companyUserIds.Contains(c.UserId))
				.ToDictionary(c => c.UserId, c => c.CompanyName);
			var emails = _db.Users
				.Where(u => companyUserIds.Contains(u.Id))
				.ToDictionary(u => u.Id, u => u.Email);

			var summaries = jobGroups
				.Select(g => new CompanySummary
				{
					CompanyUserId = g.CompanyUserId,
					CompanyName = companyNames.ContainsKey(g.CompanyUserId) ? companyNames[g.CompanyUserId] : "(Unknown)",
					Email = emails.ContainsKey(g.CompanyUserId) ? emails[g.CompanyUserId] : string.Empty,
					JobsCount = g.JobsCount,
					LastPostedUtc = g.LastPostedUtc
				})
				.OrderByDescending(s => s.LastPostedUtc)
				.ToList();

			ViewBag.College = ownedCollege;
			return View(summaries);
		}

		[HttpGet]
		public IActionResult CompanyJobs(string id)
		{
			var user = _userManager.GetUserAsync(User).Result;
			if (user == null || user.UserType != UserType.College)
			{
				return Forbid();
			}

			if (string.IsNullOrWhiteSpace(id))
			{
				return BadRequest();
			}

			var ownedCollege = _db.Colleges.FirstOrDefault(c => c.CollegeUserId == user.Id)?.Name;
			if (string.IsNullOrWhiteSpace(ownedCollege))
			{
				TempData["Error"] = "Set up your college profile first.";
				return RedirectToAction("Profile");
			}

			var companyName = _db.Companies.FirstOrDefault(c => c.UserId == id)?.CompanyName ?? "(Unknown)";
			var jobs = _db.JobPostings
				.Where(j => j.CompanyUserId == id && (j.CollegeName == ownedCollege || j.CollegeName == "All Colleges"))
				.OrderByDescending(j => j.CreatedAtUtc)
				.ToList();

			ViewBag.CompanyName = companyName;
			ViewBag.College = ownedCollege;
			return View(jobs);
		}

		[HttpGet]
		public IActionResult JobDetails(int id)
		{
			var user = _userManager.GetUserAsync(User).Result;
			if (user == null || user.UserType != UserType.College)
			{
				return Forbid();
			}

			var ownedCollege = _db.Colleges.FirstOrDefault(c => c.CollegeUserId == user.Id)?.Name;
			if (string.IsNullOrWhiteSpace(ownedCollege))
			{
				TempData["Error"] = "Set up your college profile first.";
				return RedirectToAction("Profile");
			}

			var job = _db.JobPostings
				.Include(j => j.CompanyUser)
				.FirstOrDefault(j => j.Id == id && (j.CollegeName == ownedCollege || j.CollegeName == "All Colleges"));

			if (job == null)
			{
				return NotFound();
			}

			// Get company name
			var companyName = _db.Companies.FirstOrDefault(c => c.UserId == job.CompanyUserId)?.CompanyName ?? "(Unknown)";
			ViewBag.CompanyName = companyName;
			ViewBag.College = ownedCollege;

			return View(job);
		}

		[HttpGet]
		public IActionResult Profile()
		{
			var user = _userManager.GetUserAsync(User).Result;
			if (user == null || user.UserType != UserType.College)
			{
				return Forbid();
			}
			var college = _db.Colleges.FirstOrDefault(c => c.CollegeUserId == user.Id) ?? new College { CollegeUserId = user.Id };
			return View(college);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Profile(College model)
		{
			var user = _userManager.GetUserAsync(User).Result;
			if (user == null || user.UserType != UserType.College)
			{
				return Forbid();
			}
			if (!ModelState.IsValid)
			{
				return View(model);
			}
			var college = _db.Colleges.FirstOrDefault(c => c.CollegeUserId == user.Id);
			if (college == null)
			{
				college = new College
				{
					CollegeUserId = user.Id,
					Name = model.Name,
					WebsiteUrl = model.WebsiteUrl,
					City = model.City,
					State = model.State
				};
				_db.Colleges.Add(college);
			}
			else
			{
				college.Name = model.Name;
				college.WebsiteUrl = model.WebsiteUrl;
				college.City = model.City;
				college.State = model.State;
			}
			_db.SaveChanges();
			TempData["Success"] = "Profile updated.";
			return RedirectToAction("Profile");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ApproveStudent(int id)
		{
			var student = _db.Students.FirstOrDefault(s => s.Id == id);
			if (student == null)
			{
				return NotFound();
			}
			student.IsApproved = true;
			_db.SaveChanges();
			return RedirectToAction("Students", new { college = student.CollegeName });
		}
	}
}


