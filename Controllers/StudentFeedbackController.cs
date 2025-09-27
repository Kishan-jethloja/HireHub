using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlacementManagementSystem.Data;
using PlacementManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PlacementManagementSystem.Controllers
{
    [Authorize]
    public class StudentFeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentFeedbackController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.UserType != UserType.Student)
            {
                return Forbid();
            }

            // Get applications where student has been accepted or rejected
            var applications = await _context.Applications
                .Where(a => a.StudentUserId == user.Id && a.Status != ApplicationStatus.Pending)
                .Include(a => a.JobPosting)
                .ThenInclude(j => j.CompanyUser)
                .Select(a => new
                {
                    ApplicationId = a.Id,
                    JobTitle = a.JobPosting.Title,
                    CompanyName = _context.Companies
                        .Where(c => c.UserId == a.JobPosting.CompanyUserId)
                        .Select(c => c.CompanyName)
                        .FirstOrDefault(),
                    Status = a.Status,
                    AppliedDate = a.CreatedAtUtc,
                    HasFeedback = _context.Feedbacks.Any(f => f.AuthorUserId == user.Id && f.JobPostingId == a.JobPostingId)
                })
                .ToListAsync();

            return View(applications);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int applicationId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.UserType != UserType.Student)
            {
                return Forbid();
            }

            // Verify the application belongs to the student and is decided
            var application = await _context.Applications
                .Where(a => a.Id == applicationId && a.StudentUserId == user.Id && a.Status != ApplicationStatus.Pending)
                .Include(a => a.JobPosting)
                .ThenInclude(j => j.CompanyUser)
                .FirstOrDefaultAsync();

            if (application == null)
            {
                return NotFound();
            }

            // Check if feedback already exists
            var existingFeedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.AuthorUserId == user.Id);

            if (existingFeedback != null)
            {
                TempData["Error"] = "You have already provided feedback for this application.";
                return RedirectToAction("Index");
            }

            // Get company name
            var companyName = await _context.Companies
                .Where(c => c.UserId == application.JobPosting.CompanyUserId)
                .Select(c => c.CompanyName)
                .FirstOrDefaultAsync();

            ViewBag.Application = application;
            ViewBag.CompanyName = companyName;
            ViewBag.JobTitle = application.JobPosting.Title;
            ViewBag.Status = application.Status;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(int applicationId, string message, int rating)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.UserType != UserType.Student)
            {
                return Forbid();
            }

            // Verify the application belongs to the student and is decided
            var application = await _context.Applications
                .Where(a => a.Id == applicationId && a.StudentUserId == user.Id && a.Status != ApplicationStatus.Pending)
                .Include(a => a.JobPosting)
                .ThenInclude(j => j.CompanyUser)
                .FirstOrDefaultAsync();

            if (application == null)
            {
                return NotFound();
            }

            // Check if feedback already exists for this specific job
            var existingFeedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.AuthorUserId == user.Id && f.JobPostingId == application.JobPostingId);

            if (existingFeedback != null)
            {
                TempData["Error"] = "You have already provided feedback for this job.";
                return RedirectToAction("Jobs", "Student");
            }

            // Validate input
            if (string.IsNullOrWhiteSpace(message) || rating < 1 || rating > 5)
            {
                TempData["Error"] = "Please provide valid feedback details.";
                return RedirectToAction("Create", new { applicationId });
            }

            // Get company ID
            var companyId = await _context.Companies
                .Where(c => c.UserId == application.JobPosting.CompanyUserId)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            // Create feedback (without ApplicationId for now)
            var feedback = new Feedback
            {
                AuthorUserId = user.Id,
                // ApplicationId = applicationId, // Temporarily disabled
                JobPostingId = application.JobPostingId, // Link to specific job
                TargetType = FeedbackTargetType.Company,
                TargetCompanyId = companyId,
                Subject = "Feedback", // Default subject since it's required in the model
                Message = message.Trim(),
                Rating = rating,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thank you for your feedback!";
            return RedirectToAction("Jobs", "Student");
        }

        [HttpGet]
        public async Task<IActionResult> GetApplicationId(int jobId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.UserType != UserType.Student)
            {
                return Json(new { error = "Unauthorized" });
            }

            var application = await _context.Applications
                .Where(a => a.JobPostingId == jobId && a.StudentUserId == user.Id && a.Status != ApplicationStatus.Pending)
                .FirstOrDefaultAsync();

            if (application == null)
            {
                return Json(new { error = "Application not found" });
            }

            return Json(new { applicationId = application.Id });
        }
    }
}
