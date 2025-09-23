using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using PlacementManagementSystem.Data;
using PlacementManagementSystem.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace PlacementManagementSystem.Controllers
{
	[Authorize]
	public class StudentController : Controller
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;

		public StudentController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
		{
			_db = db;
			_userManager = userManager;
		}

		[HttpGet]
		public async Task<IActionResult> Apply(int id)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null || user.UserType != UserType.Student)
			{
				return Forbid();
			}
			var job = _db.JobPostings.FirstOrDefault(j => j.Id == id);
			if (job == null)
			{
				return NotFound();
			}
			ViewBag.Job = job;
			var existing = _db.Applications.FirstOrDefault(a => a.JobPostingId == id && a.StudentUserId == user.Id);
			if (existing != null)
			{
				return View(existing);
			}
			return View(new Application());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Apply(int id, Application model, IFormFile resumeFile)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null || user.UserType != UserType.Student)
			{
				return Forbid();
			}
			var job = _db.JobPostings.FirstOrDefault(j => j.Id == id);
			if (job == null)
			{
				return NotFound();
			}

			ModelState.Remove("StudentUserId");
			ModelState.Remove("JobPostingId");
			if (!model.TermsAccepted)
			{
				ModelState.AddModelError("TermsAccepted", "You must accept the terms and conditions.");
			}

			// Upsert: create new or update existing application for this job
			var existing = _db.Applications.FirstOrDefault(a => a.JobPostingId == job.Id && a.StudentUserId == user.Id);
			if (!ModelState.IsValid)
			{
				ViewBag.Job = job;
				return View(model);
			}

			Application application;
			if (existing == null)
			{
				application = new Application
				{
					JobPostingId = job.Id,
					StudentUserId = user.Id
				};
				_db.Applications.Add(application);
			}
			else
			{
				application = existing;
			}

			application.ApplicantName = model.ApplicantName;
			application.ApplicantEmail = model.ApplicantEmail;
			application.CollegeId = model.CollegeId;
			application.LinkedInUrl = model.LinkedInUrl;
			application.GithubUrl = model.GithubUrl;
			application.Gender = model.Gender;
			application.CoverLetter = model.CoverLetter;
			application.TermsAccepted = model.TermsAccepted;

			if (resumeFile != null && resumeFile.Length > 0)
			{
				var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "applications");
				if (!Directory.Exists(uploadsDir))
				{
					Directory.CreateDirectory(uploadsDir);
				}
				var fileName = $"app_{user.Id}_{Guid.NewGuid():N}.pdf";
				var filePath = Path.Combine(uploadsDir, fileName);
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await resumeFile.CopyToAsync(stream);
				}
				application.ResumePath = $"/uploads/applications/{fileName}";
			}

			await _db.SaveChangesAsync();
			TempData["AppSuccess"] = "Application submitted.";
			return RedirectToAction("Jobs");
		}
		public async Task<IActionResult> Jobs()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null || user.UserType != UserType.Student)
			{
				return Forbid();
			}

			var student = _db.Students.FirstOrDefault(s => s.UserId == user.Id);
			if (student == null)
			{
				TempData["Error"] = "Complete your profile first.";
				return RedirectToAction("Profile");
			}

			// Block job listings until college has approved the student
			if (!student.IsApproved)
			{
				TempData["Error"] = $"Pending college confirmation for {student.CollegeName}.";
				return RedirectToAction("Details");
			}

			var nowUtc = DateTime.UtcNow;
			var jobs = _db.JobPostings
				.Where(j => j.CollegeName == student.CollegeName && (j.ApplyByUtc == null || j.ApplyByUtc >= nowUtc))
				.OrderByDescending(j => j.CreatedAtUtc)
				.ToList();

			var appliedJobIds = _db.Applications
				.Where(a => a.StudentUserId == user.Id)
				.Select(a => a.JobPostingId)
				.ToHashSet();
			ViewBag.AppliedJobIds = appliedJobIds;

			return View(jobs);
		}

		[HttpGet]
		public async Task<IActionResult> Profile()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null || user.UserType != UserType.Student)
			{
				return Forbid();
			}

			var student = _db.Students.FirstOrDefault(s => s.UserId == user.Id) ?? new Student { UserId = user.Id };
			// Normalize placeholders to empty for first-time display
			if (student.CollegeName == "Unassigned") student.CollegeName = string.Empty;
			if (student.Department == "Unassigned") student.Department = string.Empty;
			if (student.Year == "TBD") student.Year = string.Empty;
			ViewBag.Colleges = _db.Colleges.Select(c => c.Name).OrderBy(n => n).ToList();
			return View(student);
		}

		[HttpGet]
		public async Task<IActionResult> Details()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null || user.UserType != UserType.Student)
			{
				return Forbid();
			}

			var student = _db.Students.FirstOrDefault(s => s.UserId == user.Id);
			if (student == null)
			{
				TempData["Error"] = "Complete your profile first.";
				return RedirectToAction("Profile");
			}

			ViewBag.FullName = user.FullName;
			ViewBag.Email = user.Email;
			return View(student);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Profile(Student model, IFormFile resumeFile)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null || user.UserType != UserType.Student)
			{
				return Forbid();
			}

			// UserId comes from the logged-in user, not the form
			ModelState.Remove("UserId");
			ModelState.Remove("Id");
			// StudentId is auto-generated and not submitted by the form
			ModelState.Remove("StudentId");

			if (!ModelState.IsValid)
			{
				ViewBag.Colleges = _db.Colleges.Select(c => c.Name).OrderBy(n => n).ToList();
				return View(model);
			}

			var student = _db.Students.FirstOrDefault(s => s.UserId == user.Id);
			if (student == null)
			{
				student = new Student
				{
					UserId = user.Id,
					CollegeName = model.CollegeName,
					// StudentId will be auto-generated below
					Department = model.Department,
					Year = model.Year,
					CGPA = model.CGPA,
					Skills = model.Skills,
					ResumePath = model.ResumePath
				};
				_db.Students.Add(student);
			}
			else
			{
				// If college changed, reset approval
				if (student.CollegeName != model.CollegeName)
				{
					student.IsApproved = false;
				}
				student.CollegeName = model.CollegeName;
				// StudentId remains immutable once assigned
				student.Department = model.Department;
				student.Year = model.Year;
				student.CGPA = model.CGPA;
				student.Skills = model.Skills;
			}

			// Ensure StudentId exists and is unique; generate on first save
			if (string.IsNullOrWhiteSpace(student.StudentId))
			{
				// Simple unique generator: yyyymm + 4 random digits
				// Loop to avoid rare collisions
				var prefix = DateTime.UtcNow.ToString("yyyyMM");
				var rng = new Random();
				string candidate;
				do
				{
					candidate = $"{prefix}{rng.Next(1000, 10000)}";
				}
				while (_db.Students.Any(s => s.StudentId == candidate));
				student.StudentId = candidate;
			}

			// Save resume if uploaded
			if (resumeFile != null && resumeFile.Length > 0)
			{
				var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "resumes");
				if (!Directory.Exists(uploadsDir))
				{
					Directory.CreateDirectory(uploadsDir);
				}
				var fileName = $"resume_{user.Id}_{Guid.NewGuid():N}.pdf";
				var filePath = Path.Combine(uploadsDir, fileName);
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await resumeFile.CopyToAsync(stream);
				}
				student.ResumePath = $"/uploads/resumes/{fileName}";
			}

			await _db.SaveChangesAsync();
			TempData["ProfileUpdated"] = true;
			if (!student.IsApproved && !string.IsNullOrEmpty(student.CollegeName) && student.CollegeName != "Unassigned")
			{
				TempData["Pending"] = $"Request sent to {student.CollegeName}. Awaiting approval.";
			}
			return RedirectToAction("Index", "Home");
		}
	}
}


