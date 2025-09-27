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
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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

            // Get student's college
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (student == null)
            {
                return Forbid();
            }

            var collegeName = student.CollegeName;
            if (string.IsNullOrWhiteSpace(collegeName) || collegeName == "Unassigned")
            {
                ViewBag.ErrorMessage = "You must be assigned to a college to use chat";
                return View(new List<ChatMessageViewModel>());
            }

            // Get recent messages (last 50) - filtered by college
            List<ChatMessageViewModel> messages = new List<ChatMessageViewModel>();
            
            try
            {
                // Get messages from students in the same college
                var collegeStudentIds = await _context.Students
                    .Where(s => s.CollegeName == collegeName)
                    .Select(s => s.UserId)
                    .ToListAsync();

                messages = await _context.ChatMessages
                    .Where(m => !m.IsDeleted && collegeStudentIds.Contains(m.SenderUserId))
                    .Include(m => m.SenderUser)
                    .OrderByDescending(m => m.SentAtUtc)
                    .Take(50)
                    .Select(m => new ChatMessageViewModel
                    {
                        Id = m.Id,
                        SenderName = $"{m.SenderUser.FirstName} {m.SenderUser.LastName}".Trim() != "" 
                            ? $"{m.SenderUser.FirstName} {m.SenderUser.LastName}".Trim()
                            : m.SenderUser.Email,
                        Message = m.Message,
                        SentAt = m.SentAtUtc,
                        IsOwnMessage = m.SenderUserId == user.Id
                    })
                    .OrderBy(m => m.SentAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                // If database table doesn't exist yet, return empty list
                messages = new List<ChatMessageViewModel>();
            }

            ViewBag.CurrentUserId = user.Id;
            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string message)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.UserType != UserType.Student)
            {
                return Json(new { success = false, error = "Only students can send messages" });
            }

            if (string.IsNullOrWhiteSpace(message) || message.Length > 1000)
            {
                return Json(new { success = false, error = "Message must be between 1 and 1000 characters" });
            }

            var chatMessage = new ChatMessage
            {
                SenderUserId = user.Id,
                Message = message.Trim(),
                SentAtUtc = DateTime.UtcNow
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.UserType != UserType.Student)
            {
                return Json(new { success = false, error = "Only students can delete messages" });
            }

            var message = await _context.ChatMessages.FindAsync(messageId);
            if (message == null || message.SenderUserId != user.Id)
            {
                return Json(new { success = false, error = "You can only delete your own messages" });
            }

            message.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
