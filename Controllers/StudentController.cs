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
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

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
		public async Task<IActionResult> Announcements()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null || user.UserType != UserType.Student)
			{
				return Forbid();
			}
			var eligibleJobIds = _db.Applications
				.Where(a => a.StudentUserId == user.Id && a.Status != ApplicationStatus.Rejected)
				.Select(a => a.JobPostingId)
				.Distinct()
				.ToList();
			var anns = _db.Announcements
				.Where(a => eligibleJobIds.Contains(a.JobPostingId))
				.Include(a => a.CompanyUser)
				.Include(a => a.JobPosting)
				.OrderByDescending(a => a.CreatedAtUtc)
				.ToList();
			// Map CompanyUserId -> Company.CompanyName for display
			var companyUserIds = anns.Select(a => a.CompanyUserId).Distinct().ToList();
			var companyNames = _db.Companies
				.Where(c => companyUserIds.Contains(c.UserId))
				.ToDictionary(c => c.UserId, c => c.CompanyName);
			ViewBag.CompanyNamesByUserId = companyNames;
			return View(anns);
		}

		[HttpGet]
		public async Task<IActionResult> Announcement(int id)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null || user.UserType != UserType.Student)
			{
				return Forbid();
			}
			var ann = _db.Announcements
				.Include(a => a.CompanyUser)
				.Include(a => a.JobPosting)
				.FirstOrDefault(a => a.Id == id);
			if (ann == null)
			{
				return NotFound();
			}
			// Authorization: student must have a non-rejected application for this job
			bool eligible = _db.Applications.Any(a => a.JobPostingId == ann.JobPostingId && a.StudentUserId == user.Id && a.Status != ApplicationStatus.Rejected);
			if (!eligible)
			{
				return Forbid();
			}
			// Company display name map for the view
			var name = _db.Companies.Where(c => c.UserId == ann.CompanyUserId).Select(c => c.CompanyName).FirstOrDefault();
			ViewBag.CompanyName = string.IsNullOrWhiteSpace(name) ? ann.CompanyUser?.FullName : name;
			return View(ann);
		}
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
			
			// Get company name
			var companyName = _db.Companies.FirstOrDefault(c => c.UserId == job.CompanyUserId)?.CompanyName ?? "Unknown Company";
			ViewBag.Job = job;
			ViewBag.CompanyName = companyName;
			var existing = _db.Applications.FirstOrDefault(a => a.JobPostingId == id && a.StudentUserId == user.Id);
			// Announcements for this job
			ViewBag.Announcements = _db.Announcements
				.Where(a => a.JobPostingId == id)
				.OrderByDescending(a => a.CreatedAtUtc)
				.ToList();
			// Current student's application status (if any)
			ViewBag.StudentApplicationStatus = existing?.Status;
			if (existing != null)
			{
				return View(existing);
			}

			// Pre-populate form with student profile data
			var student = _db.Students.FirstOrDefault(s => s.UserId == user.Id);
			var application = new Application();
			
			if (student != null)
			{
				// Pre-fill with student profile data
				application.ApplicantName = user.FullName;
				application.ApplicantEmail = user.Email;
				application.CollegeId = student.StudentId; // Use StudentId as CollegeId
				
				// Cover letter remains blank for students to fill manually
			}
			else
			{
				// Fallback to user data if student profile not found
				application.ApplicantName = user.FullName;
				application.ApplicantEmail = user.Email;
			}

			return View(application);
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
				// Get company name for validation error display
				var companyName = _db.Companies.FirstOrDefault(c => c.UserId == job.CompanyUserId)?.CompanyName ?? "Unknown Company";
				ViewBag.CompanyName = companyName;
				// Repopulate announcements and current status on validation error
				var current = _db.Applications.FirstOrDefault(a => a.JobPostingId == job.Id && a.StudentUserId == user.Id);
				ViewBag.Announcements = _db.Announcements
					.Where(a => a.JobPostingId == job.Id)
					.OrderByDescending(a => a.CreatedAtUtc)
					.ToList();
				ViewBag.StudentApplicationStatus = current?.Status;
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
				.Where(j => j.MinimumCPI == null || j.MinimumCPI <= student.CGPA) // Filter by CPI requirement
				.OrderByDescending(j => j.CreatedAtUtc)
				.ToList();

			var appliedJobIds = _db.Applications
				.Where(a => a.StudentUserId == user.Id)
				.Select(a => a.JobPostingId)
				.ToHashSet();
			ViewBag.AppliedJobIds = appliedJobIds;

			// Get application status for each job
			var applicationStatuses = _db.Applications
				.Where(a => a.StudentUserId == user.Id)
				.ToDictionary(a => a.JobPostingId, a => a.Status);
			ViewBag.ApplicationStatuses = applicationStatuses;

			// Get jobs where feedback has been given
			var feedbackGivenJobs = _db.Feedbacks
				.Where(f => f.AuthorUserId == user.Id && f.JobPostingId != null)
				.Select(f => f.JobPostingId.Value)
				.ToHashSet();
			ViewBag.FeedbackGivenJobs = feedbackGivenJobs;

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

			// Announcements for jobs this student applied to and not rejected
			var eligibleJobIds = _db.Applications
				.Where(a => a.StudentUserId == user.Id && a.Status != ApplicationStatus.Rejected)
				.Select(a => a.JobPostingId)
				.Distinct()
				.ToList();
			ViewBag.ProfileAnnouncements = _db.Announcements
				.Where(a => eligibleJobIds.Contains(a.JobPostingId))
				.OrderByDescending(a => a.CreatedAtUtc)
				.Take(10)
				.ToList();
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


