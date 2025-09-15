using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlacementManagementSystem.Models;
using PlacementManagementSystem.ViewModels;
using System.Threading.Tasks;
using PlacementManagementSystem.Data;
using System;

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
			// Additional server-side validation for student
			if (model.UserType == UserType.Student)
			{
				if (string.IsNullOrWhiteSpace(model.CollegeName))
				{
					ModelState.AddModelError(nameof(model.CollegeName), "College Name is required for students.");
				}
				if (string.IsNullOrWhiteSpace(model.StudentId))
				{
					ModelState.AddModelError(nameof(model.StudentId), "Student ID is required for students.");
				}
				if (string.IsNullOrWhiteSpace(model.Department))
				{
					ModelState.AddModelError(nameof(model.Department), "Department is required for students.");
				}
				if (string.IsNullOrWhiteSpace(model.Year))
				{
					ModelState.AddModelError(nameof(model.Year), "Academic Year is required for students.");
				}
			}

			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var user = new ApplicationUser
			{
				UserName = model.Email,
				Email = model.Email,
				FirstName = model.FirstName,
				LastName = model.LastName,
				UserType = model.UserType
			};

			var result = await _userManager.CreateAsync(user, model.Password);

			if (result.Succeeded)
			{
				try
				{
					if (model.UserType == UserType.Student)
					{
						var student = new Student
						{
							UserId = user.Id,
							CollegeName = model.CollegeName,
							StudentId = model.StudentId,
							Department = model.Department,
							Year = model.Year,
							CGPA = model.CGPA ?? 0
						};
						_db.Students.Add(student);
						await _db.SaveChangesAsync();
					}
					else if (model.UserType == UserType.Company)
					{
						// Placeholder for company profile creation in future
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
	}
}
