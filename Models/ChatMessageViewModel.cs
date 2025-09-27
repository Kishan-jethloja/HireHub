using System;

namespace PlacementManagementSystem.Models
{
    public class ChatMessageViewModel
    {
        public int Id { get; set; }
        public string SenderName { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsOwnMessage { get; set; }
    }
}
