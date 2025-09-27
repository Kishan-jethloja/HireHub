using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlacementManagementSystem.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        public string SenderUserId { get; set; }
        [ForeignKey("SenderUserId")]
        public ApplicationUser SenderUser { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; }

        public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;
    }
}
