using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PlacementManagementSystem.Data;
using PlacementManagementSystem.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PlacementManagementSystem.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task JoinChat()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return;

            var user = await _context.Users.FindAsync(userId);
            if (user?.UserType != UserType.Student)
            {
                await Clients.Caller.SendAsync("Error", "Only students can access chat");
                return;
            }

            // Get student's college
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null)
            {
                await Clients.Caller.SendAsync("Error", "Student profile not found");
                return;
            }

            var collegeName = student.CollegeName;
            if (string.IsNullOrWhiteSpace(collegeName) || collegeName == "Unassigned")
            {
                await Clients.Caller.SendAsync("Error", "You must be assigned to a college to use chat");
                return;
            }

            // Join college-specific group
            var groupName = $"College_{collegeName.Replace(" ", "_")}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("JoinedChat", $"You have joined the {collegeName} chat");
        }

        public async Task SendMessage(string message)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return;

            var user = await _context.Users.FindAsync(userId);
            if (user?.UserType != UserType.Student)
            {
                await Clients.Caller.SendAsync("Error", "Only students can send messages");
                return;
            }

            // Get student's college
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null)
            {
                await Clients.Caller.SendAsync("Error", "Student profile not found");
                return;
            }

            var collegeName = student.CollegeName;
            if (string.IsNullOrWhiteSpace(collegeName) || collegeName == "Unassigned")
            {
                await Clients.Caller.SendAsync("Error", "You must be assigned to a college to send messages");
                return;
            }

            if (string.IsNullOrWhiteSpace(message) || message.Length > 1000)
            {
                await Clients.Caller.SendAsync("Error", "Message must be between 1 and 1000 characters");
                return;
            }

            var chatMessage = new ChatMessage
            {
                SenderUserId = userId,
                Message = message.Trim(),
                SentAtUtc = DateTime.UtcNow
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            var senderName = $"{user.FirstName} {user.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(senderName))
                senderName = user.Email;

            // Send message only to students from the same college
            var groupName = $"College_{collegeName.Replace(" ", "_")}";
            await Clients.Group(groupName).SendAsync("ReceiveMessage", new
            {
                Id = chatMessage.Id,
                SenderUserId = chatMessage.SenderUserId,
                SenderName = senderName,
                Message = chatMessage.Message,
                SentAt = chatMessage.SentAtUtc
            });
        }

        public async Task DeleteMessage(int messageId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return;

            var message = await _context.ChatMessages.FindAsync(messageId);
            if (message == null || message.SenderUserId != userId)
            {
                await Clients.Caller.SendAsync("Error", "You can only delete your own messages");
                return;
            }

            // Get the sender's college to send deletion to the right group
            var sender = await _context.Users.FindAsync(message.SenderUserId);
            var senderStudent = await _context.Students.FirstOrDefaultAsync(s => s.UserId == message.SenderUserId);
            if (senderStudent != null && !string.IsNullOrWhiteSpace(senderStudent.CollegeName))
            {
                var groupName = $"College_{senderStudent.CollegeName.Replace(" ", "_")}";
                message.IsDeleted = true;
                await _context.SaveChangesAsync();
                await Clients.Group(groupName).SendAsync("MessageDeleted", messageId);
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Remove from all possible college groups (cleanup)
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                if (student != null && !string.IsNullOrWhiteSpace(student.CollegeName))
                {
                    var groupName = $"College_{student.CollegeName.Replace(" ", "_")}";
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
