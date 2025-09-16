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

			var nowUtc = DateTime.UtcNow;
			var jobs = _db.JobPostings
				.Where(j => j.CollegeName == student.CollegeName && (j.ApplyByUtc == null || j.ApplyByUtc >= nowUtc))
				.OrderByDescending(j => j.CreatedAtUtc)
				.ToList();

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

			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var student = _db.Students.FirstOrDefault(s => s.UserId == user.Id);
			if (student == null)
			{
				student = new Student
				{
					UserId = user.Id,
					CollegeName = model.CollegeName,
					StudentId = model.StudentId,
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
				student.CollegeName = model.CollegeName;
				student.StudentId = model.StudentId;
				student.Department = model.Department;
				student.Year = model.Year;
				student.CGPA = model.CGPA;
				student.Skills = model.Skills;
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
			return RedirectToAction("Index", "Home");
		}
	}
}


