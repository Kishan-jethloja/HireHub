using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlacementManagementSystem.Models;
using PlacementManagementSystem.ViewModels;
using System.Threading.Tasks;
using PlacementManagementSystem.Data;
using System;
using System.IO;
using System.Linq;

namespace PlacementManagementSystem.Controllers
{
	public class AccountController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly ApplicationDbContext _db;

		public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext db)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_db = db;
		}

		[HttpGet]
		public IActionResult Login(string returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;

			if (ModelState.IsValid)
			{
				var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
				if (result.Succeeded)
				{
					var user = await _userManager.FindByEmailAsync(model.Email);
					if (user != null && user.UserType == UserType.Company)
					{
						return RedirectToAction("MyJobs", "Company");
					}
					if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
					{
						return Redirect(returnUrl);
					}
					else
					{
						return RedirectToAction("Index", "Home");
					}
				}
				else
				{
					ModelState.AddModelError(string.Empty, "Invalid login attempt.");
					return View(model);
				}
			}

			return View(model);
		}

		[HttpGet]
		public IActionResult Register()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{

			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var isPerson = model.UserType == UserType.Student;
			var user = new ApplicationUser
			{
				UserName = model.Email,
				Email = model.Email,
				FirstName = isPerson ? (model.FirstName ?? "") : "",
				LastName = isPerson ? (model.LastName ?? "") : "",
				UserType = model.UserType
			};

			var result = await _userManager.CreateAsync(user, model.Password);

			if (result.Succeeded)
			{
				try
				{
					if (model.UserType == UserType.College)
					{
						// Ensure a College record exists for the provided name
						var existingCollege = _db.Colleges.FirstOrDefault(c => c.Name == model.CompanyName);
						if (existingCollege == null && !string.IsNullOrWhiteSpace(model.CompanyName))
						{
							_db.Colleges.Add(new College
							{
								Name = model.CompanyName,
								WebsiteUrl = model.Website,
								CollegeUserId = user.Id
							});
							await _db.SaveChangesAsync();
						}
					}
					else if (model.UserType == UserType.Company)
					{
						// Placeholder for company profile creation in future
					}
					else if (model.UserType == UserType.Student)
					{
						// Create a Student record immediately with an auto-generated StudentId
						// Generate unique StudentId similar to controller logic
						var prefix = DateTime.UtcNow.ToString("yyyyMM");
						var rng = new Random();
						string candidate;
						do
						{
							candidate = $"{prefix}{rng.Next(1000, 10000)}";
						}
						while (_db.Students.Any(s => s.StudentId == candidate));
						var student = new Student
						{
							UserId = user.Id,
							StudentId = candidate,
							CollegeName = "Unassigned",
							Department = "",
							Year = "",
							CGPA = 0,
							IsApproved = false
						};
						_db.Students.Add(student);
						await _db.SaveChangesAsync();
					}
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, $"Failed to save profile: {ex.Message}");
					// Undo user creation if profile save fails
					await _userManager.DeleteAsync(user);
					return View(model);
				}

				TempData["Success"] = "Registration successful. Please login.";
				return RedirectToAction("Login");
			}

			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}

		[HttpGet]
		public IActionResult AccessDenied(string returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;
			return View();
		}
	}
}
