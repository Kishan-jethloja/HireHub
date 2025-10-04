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
		public IActionResult Login(LoginViewModel model, string returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;

			if (ModelState.IsValid)
			{
				var result = _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false).Result;
				if (result.Succeeded)
				{
					var user = _userManager.FindByEmailAsync(model.Email).Result;
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
		public IActionResult Register(RegisterViewModel model)
		{
			// Remove validation errors for fields not relevant to current user type BEFORE validation
			if (model.UserType == UserType.Student)
			{
				ModelState.Remove("CompanyName");
				ModelState.Remove("CollegeName");
				ModelState.Remove("City");
				ModelState.Remove("State");
			}
			else if (model.UserType == UserType.Company)
			{
				ModelState.Remove("FirstName");
				ModelState.Remove("LastName");
				ModelState.Remove("CollegeName");
				ModelState.Remove("City");
				ModelState.Remove("State");
			}
			else if (model.UserType == UserType.College)
			{
				ModelState.Remove("FirstName");
				ModelState.Remove("LastName");
				ModelState.Remove("CompanyName");
			}

			// Debug: Log model values
			System.Diagnostics.Debug.WriteLine($"UserType: {model.UserType}");
			System.Diagnostics.Debug.WriteLine($"Email: {model.Email}");
			System.Diagnostics.Debug.WriteLine($"FirstName: {model.FirstName}");
			System.Diagnostics.Debug.WriteLine($"LastName: {model.LastName}");
			
			// Check if UserType is selected
			if (model.UserType == 0) // Default value means not selected
			{
				ModelState.AddModelError("UserType", "Please select your user type");
			}

			// Always required fields
			if (string.IsNullOrWhiteSpace(model.Email))
			{
				ModelState.AddModelError("Email", "Please enter your email address");
			}
			if (string.IsNullOrWhiteSpace(model.Password))
			{
				ModelState.AddModelError("Password", "Please enter a password");
			}

			// Manual required checks by role
			if (model.UserType == UserType.Student)
			{
				if (string.IsNullOrWhiteSpace(model.FirstName))
				{
					ModelState.AddModelError("FirstName", "Please enter your first name");
				}
				if (string.IsNullOrWhiteSpace(model.LastName))
				{
					ModelState.AddModelError("LastName", "Please enter your last name");
				}
			}
			else if (model.UserType == UserType.Company)
			{
				if (string.IsNullOrWhiteSpace(model.CompanyName))
				{
					ModelState.AddModelError("CompanyName", "Please enter your company name");
				}
			}
			else if (model.UserType == UserType.College)
			{
				// Fallback to raw form value in case model binding failed
				if (string.IsNullOrWhiteSpace(model.CollegeName))
				{
					var rawCollegeName = (Request?.Form["CollegeName"].ToString() ?? string.Empty).Trim();
					if (!string.IsNullOrWhiteSpace(rawCollegeName))
					{
						model.CollegeName = rawCollegeName;
					}
				}
				if (string.IsNullOrWhiteSpace(model.CollegeName))
				{
					ModelState.AddModelError("CollegeName", "Please enter your college name");
				}
				if (string.IsNullOrWhiteSpace(model.City))
				{
					ModelState.AddModelError("City", "Please enter your city");
				}
				if (string.IsNullOrWhiteSpace(model.State))
				{
					ModelState.AddModelError("State", "Please enter your state");
				}
			}

			// Debug: Log validation errors
			if (!ModelState.IsValid)
			{
				foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
				{
					System.Diagnostics.Debug.WriteLine($"Validation Error: {error.ErrorMessage}");
				}
				return View(model);
			}

			// Prevent duplicate email registration (unique UserName/Email)
			var existingUser = _userManager.FindByEmailAsync(model.Email).Result;
			if (existingUser != null)
			{
				ModelState.AddModelError("Email", "An account with this email already exists.");
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

			var result = _userManager.CreateAsync(user, model.Password).Result;

			if (result.Succeeded)
			{
				try
				{
					if (model.UserType == UserType.College)
					{
						// Always create a College record owned by this user
						var owned = _db.Colleges.FirstOrDefault(c => c.CollegeUserId == user.Id);
						if (owned == null)
						{
							_db.Colleges.Add(new College
							{
								CollegeUserId = user.Id,
								Name = string.IsNullOrWhiteSpace(model.CollegeName) ? "" : model.CollegeName.Trim(),
								WebsiteUrl = string.IsNullOrWhiteSpace(model.Website) ? "" : model.Website.Trim(),
								City = string.IsNullOrWhiteSpace(model.City) ? "" : model.City.Trim(),
								State = string.IsNullOrWhiteSpace(model.State) ? "" : model.State.Trim()
							});
						}
						else
						{
							owned.Name = string.IsNullOrWhiteSpace(model.CollegeName) ? owned.Name : model.CollegeName.Trim();
							owned.WebsiteUrl = string.IsNullOrWhiteSpace(model.Website) ? owned.WebsiteUrl : model.Website.Trim();
							owned.City = string.IsNullOrWhiteSpace(model.City) ? owned.City : model.City.Trim();
							owned.State = string.IsNullOrWhiteSpace(model.State) ? owned.State : model.State.Trim();
						}
						_db.SaveChanges();
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
						_db.SaveChanges();
					}
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, $"Failed to save profile: {ex.Message}");
					// Undo user creation if profile save fails
					_userManager.DeleteAsync(user).Wait();
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
		public IActionResult Logout()
		{
			_signInManager.SignOutAsync().Wait();
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
