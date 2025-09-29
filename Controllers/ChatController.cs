using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlacementManagementSystem.Data;
using PlacementManagementSystem.Models;
using PlacementManagementSystem.ViewModels;
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

        public IActionResult Index(bool refresh = false)
        {
            var userId = _userManager.GetUserId(User);
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
            if (user?.UserType != UserType.Student)
            {
                return Forbid();
            }

            // Get student's college
            var student = _context.Students.FirstOrDefault(s => s.UserId == user.Id);
            if (student == null)
            {
                return Forbid();
            }

            // Refresh student data to get latest college information
            _context.Entry(student).Reload();

            // Check if student is approved by college
            if (!student.IsApproved)
            {
                if (student.CollegeName == "Unassigned")
                {
                    ViewBag.ErrorMessage = "Please choose your correct college. Your previous college registration was rejected.";
                }
                else
                {
                    ViewBag.ErrorMessage = $"Chat access is restricted until college approval. Pending confirmation for {student.CollegeName}.";
                }
                return View(new List<ChatMessageViewModel>());
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
            // Filter by college
            
            // Get messages from students in the same college (case-insensitive comparison)
            var collegeStudentIds = _context.Students
                .Where(s => s.CollegeName != null && s.CollegeName.Trim().ToLower() == collegeName.Trim().ToLower())
                .Select(s => s.UserId)
                .ToList();
            
            // Build message list

                messages = _context.ChatMessages
                    .Where(m => !m.IsDeleted && collegeStudentIds.Contains(m.SenderUserId))
                    .Include(m => m.SenderUser)
                    .OrderByDescending(m => m.SentAtUtc)
                    .Take(50)
                    .Select(m => new ChatMessageViewModel
                    {
                        Id = m.Id,
                        SenderUserId = m.SenderUserId,
                        SenderName = $"{m.SenderUser.FirstName} {m.SenderUser.LastName}".Trim() != "" 
                            ? $"{m.SenderUser.FirstName} {m.SenderUser.LastName}".Trim()
                            : m.SenderUser.Email,
                        Message = m.Message,
                        SentAt = m.SentAtUtc,
                        IsOwnMessage = m.SenderUserId == user.Id
                    })
                    .OrderBy(m => m.SentAt)
                    .ToList();
                
                // Messages ready
            }
            catch (Exception)
            {
                // If database table doesn't exist yet, return empty list
                messages = new List<ChatMessageViewModel>();
            }

            ViewBag.CurrentUserId = user.Id;
            return View(messages);
        }

        [HttpGet]
        public IActionResult DebugChatState()
        {
            var userId = _userManager.GetUserId(User);
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
            if (user?.UserType != UserType.Student)
            {
                return Forbid();
            }

            var student = _context.Students.FirstOrDefault(s => s.UserId == user.Id);
            if (student == null)
            {
                return Json(new { error = "Student not found" });
            }

            // Refresh student data
            _context.Entry(student).Reload();

            // Get all students in the same college
            var collegeStudents = _context.Students
                .Where(s => s.CollegeName == student.CollegeName)
                .Select(s => new { s.Id, s.UserId, s.CollegeName, s.IsApproved })
                .ToList();

            // Get all messages from this student
            var allMessages = _context.ChatMessages
                .Where(m => m.SenderUserId == user.Id)
                .Select(m => new { m.Id, m.Message, m.SentAtUtc, m.IsDeleted })
                .OrderByDescending(m => m.SentAtUtc)
                .ToList();

            return Json(new
            {
                student = new { student.Id, student.CollegeName, student.IsApproved },
                collegeStudents = collegeStudents,
                allMessages = allMessages,
                totalMessages = allMessages.Count
            });
        }

        [HttpPost]
        public IActionResult ClearOldMessages()
        {
            var userId = _userManager.GetUserId(User);
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
            if (user?.UserType != UserType.Student)
            {
                return Forbid();
            }

            var student = _context.Students.FirstOrDefault(s => s.UserId == user.Id);
            if (student == null)
            {
                return Json(new { error = "Student not found" });
            }

            // Get all messages from this student
            var oldMessages = _context.ChatMessages
                .Where(m => m.SenderUserId == user.Id)
                .ToList();

            // Remove all old messages
            _context.ChatMessages.RemoveRange(oldMessages);
            _context.SaveChanges();

            return Json(new { success = true, message = $"Cleared {oldMessages.Count} old messages" });
        }

        [HttpPost]
        public IActionResult SendMessage(string message)
        {
            var userId = _userManager.GetUserId(User);
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
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
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult DeleteMessage(int messageId)
        {
            var userId = _userManager.GetUserId(User);
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
            if (user?.UserType != UserType.Student)
            {
                return Json(new { success = false, error = "Only students can delete messages" });
            }

            var message = _context.ChatMessages.FirstOrDefault(m => m.Id == messageId);
            if (message == null || message.SenderUserId != user.Id)
            {
                return Json(new { success = false, error = "You can only delete your own messages" });
            }

            message.IsDeleted = true;
            _context.SaveChanges();

            return Json(new { success = true });
        }
    }
}
