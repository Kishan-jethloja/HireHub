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

        public async Task<IActionResult> Index(bool refresh = false)
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

            // Refresh student data to get latest college information
            await _context.Entry(student).ReloadAsync();

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
            // Debug: Log the college name being used for filtering
            System.Diagnostics.Debug.WriteLine($"Filtering chat messages for college: {collegeName}");
            System.Diagnostics.Debug.WriteLine($"Student ID: {student.Id}, User ID: {user.Id}");
            
            // Get messages from students in the same college (case-insensitive comparison)
            var collegeStudentIds = await _context.Students
                .Where(s => s.CollegeName != null && s.CollegeName.Trim().ToLower() == collegeName.Trim().ToLower())
                .Select(s => s.UserId)
                .ToListAsync();
            
            // Debug: Log how many students are in this college
            System.Diagnostics.Debug.WriteLine($"Found {collegeStudentIds.Count} students in college: {collegeName}");
            System.Diagnostics.Debug.WriteLine($"College student IDs: {string.Join(", ", collegeStudentIds)}");

                messages = await _context.ChatMessages
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
                    .ToListAsync();
                
                // Debug: Log how many messages were retrieved
                System.Diagnostics.Debug.WriteLine($"Retrieved {messages.Count} messages for college: {collegeName}");
                
                // Debug: Log details of each message
                foreach (var msg in messages)
                {
                    System.Diagnostics.Debug.WriteLine($"Message from {msg.SenderName} (ID: {msg.SenderUserId}): {msg.Message}");
                }
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
        public async Task<IActionResult> DebugChatState()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.UserType != UserType.Student)
            {
                return Forbid();
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (student == null)
            {
                return Json(new { error = "Student not found" });
            }

            // Refresh student data
            await _context.Entry(student).ReloadAsync();

            // Get all students in the same college
            var collegeStudents = await _context.Students
                .Where(s => s.CollegeName == student.CollegeName)
                .Select(s => new { s.Id, s.UserId, s.CollegeName, s.IsApproved })
                .ToListAsync();

            // Get all messages from this student
            var allMessages = await _context.ChatMessages
                .Where(m => m.SenderUserId == user.Id)
                .Select(m => new { m.Id, m.Message, m.SentAtUtc, m.IsDeleted })
                .OrderByDescending(m => m.SentAtUtc)
                .ToListAsync();

            return Json(new
            {
                student = new { student.Id, student.CollegeName, student.IsApproved },
                collegeStudents = collegeStudents,
                allMessages = allMessages,
                totalMessages = allMessages.Count
            });
        }

        [HttpPost]
        public async Task<IActionResult> ClearOldMessages()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.UserType != UserType.Student)
            {
                return Forbid();
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (student == null)
            {
                return Json(new { error = "Student not found" });
            }

            // Get all messages from this student
            var oldMessages = await _context.ChatMessages
                .Where(m => m.SenderUserId == user.Id)
                .ToListAsync();

            // Remove all old messages
            _context.ChatMessages.RemoveRange(oldMessages);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Cleared {oldMessages.Count} old messages" });
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
