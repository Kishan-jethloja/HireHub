using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlacementManagementSystem.Models
{
	public class Announcement
	{
		public int Id { get; set; }

		[Required]
		public int JobPostingId { get; set; }
		[ForeignKey("JobPostingId")]
		public JobPosting JobPosting { get; set; }

		[Required]
		public string CompanyUserId { get; set; }
		[ForeignKey("CompanyUserId")]
		public ApplicationUser CompanyUser { get; set; }

		[Required]
		[StringLength(200)]
		public string Title { get; set; }

		[Required]
		[StringLength(2000)]
		public string Message { get; set; }

		public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
	}
}


